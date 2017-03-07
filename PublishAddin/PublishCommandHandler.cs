using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Gtk;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Projects.MSBuild;
using PublishProfileManager;
using PublishProfileManager.Models;

namespace PublishAddin
{
	class PublishCommandHandler : CommandHandler
	{
		protected override void Run()
		{
			string publishSettingsContent = GetPublishSettingsFileContent();

			// profile Info
			var profileInfo = PublishProfileCreatorFactory.CreateMSDeployPublishProfileFromPublishSettings(publishSettingsContent);
			string profileName = profileInfo.Key;
			string msDeployProfileContents = profileInfo.Value;
			AddContentsToProject(profileName, msDeployProfileContents, isUserFile: false);

			//User info
			profileInfo = PublishProfileCreatorFactory.CreateUserPublishProfileFromPublishSettings(publishSettingsContent);
			profileName = profileInfo.Key;
			msDeployProfileContents = profileInfo.Value;
			AddContentsToProject(profileName, msDeployProfileContents, isUserFile: true);


			Project project = IdeApp.ProjectOperations.CurrentSelectedProject;

			// Get password from the user file.
			UserPublishProfile userProfile = new UserPublishProfile();
			userProfile.LoadModel(msDeployProfileContents);
			byte[] encryptedPassword = Convert.FromBase64String(userProfile.EncryptedPassword);
			string clearTextPassword = DataProtection.GetUnProtectedPassword(encryptedPassword);

			//Invoke Build
			Build(profileName, project, clearTextPassword);
				
		}


		private bool Build(string profileName, Project project, string password)
		{
			project.BindTask(ct => Task.Run(async delegate
			{
				try
				{
					// just display a simple status, if we have any errors we will display them in a pad afterwards
					var monitor = IdeApp.Workbench.ProgressMonitors.GetBuildProgressMonitor();
					BuildResult buildResult = null;
					IReadOnlyPropertySet properties = null;

					using (monitor)
					{
						var context = new TargetEvaluationContext();
						context.GlobalProperties.SetValue("DeployOnBuild", true);
						context.GlobalProperties.SetValue("Password", password);
						context.GlobalProperties.SetValue("PublishProfile", profileName);

						var targetResult = await project.RunTarget(monitor, 
						                                           "build", 
						                                           IdeApp.Workspace.ActiveConfiguration, 
						                                           context);
						buildResult = targetResult.BuildResult;
						properties = targetResult.Properties;

						if (buildResult.ErrorCount > 0)
						{
							foreach (var error in buildResult.Errors)
							{
								await monitor.ErrorLog.WriteLineAsync(error.ErrorText);
							}
						}
						// if there are errors, display them in a pad
						return buildResult.ErrorCount > 0;
					}

				}
				catch (Exception ex)
				{
					LoggingService.LogError("Error running resgen", ex);
				}

				return false;
			}));

			return false;
		}

		private bool AddContentsToProject(string name, string contents, bool isUserFile)
		{
			Project project = IdeApp.ProjectOperations.CurrentSelectedProject;
			string publishProfilesFolder = Path.Combine(project.GetAbsoluteChildPath(""), "Properties", "PublishProfiles");
			if (!Directory.Exists(publishProfilesFolder))
			{
				Directory.CreateDirectory(publishProfilesFolder);
			}

			string userExtension = isUserFile ? ".user": string.Empty;
			string pubXmlFile = Path.Combine(publishProfilesFolder, $"{name}.pubxml{userExtension}");
			File.WriteAllText(pubXmlFile, contents);

			IdeApp.ProjectOperations.AddFilesToProject(IdeApp.ProjectOperations.CurrentSelectedProject,
													   new string[] { pubXmlFile },
													   FilePath.Build(publishProfilesFolder));
			
			return true;
		}

		protected override void Update(CommandInfo info)
		{
			info.Enabled = IdeApp.ProjectOperations.CurrentSelectedProject != null;
		}

		private string GetPublishSettingsFileContent()
		{
			string publishSettingsContent = null;

			var fileChooser = new Gtk.FileChooserDialog("Choose a PublishSettings file",
														IdeApp.Workbench.RootWindow,
														FileChooserAction.Open,
														"Cancel", ResponseType.Cancel,
														"Download Publish Settings file", ResponseType.Help,
														"Publish to Azure", ResponseType.Accept);

			FileFilter filter = new FileFilter();
			filter.AddPattern("*.PublishSettings");
			filter.Name = "Publish Settings file";
			fileChooser.AddFilter(filter);

			try
			{
				int result = 0;
				do
				{
					result = fileChooser.Run();
					switch (result)
					{
						case (int)ResponseType.Accept:
							if (File.Exists(fileChooser.Filename))
							{
								publishSettingsContent = File.ReadAllText(fileChooser.Filename);
							}
							break;

						case (int)ResponseType.Help:
							DesktopService.ShowUrl("http://portal.azure.com");
							break;

						default:
							break;
					}
				}
				while (result == (int)ResponseType.Help);
			}
			finally
			{
				fileChooser.Destroy();
			}

			return publishSettingsContent;
		}
	}
}