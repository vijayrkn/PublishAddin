using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Evaluation;

namespace PublishProfileManager.Models
{
	public abstract class PublishProfileBase
	{
		public override string ToString()
		{
			string msDeployPublishXmlContents = null;
			using (StringWriter stringWriter = new StringWriter())
			{
				using (XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented })
				{
					Type t = GetType();
					var properties = t.GetProperties()
					   .Where(prop => prop.PropertyType == typeof(string) || prop.PropertyType == typeof(bool))
					   .OrderBy(p => p.GetCustomAttributes(typeof(DisplayAttribute), true)
					   .Cast<DisplayAttribute>()
					   .Select(a => a.Order)
					   .FirstOrDefault());

					xmlWriter.WriteStartDocument();
					xmlWriter.WriteStartElement("Project");
					xmlWriter.WriteAttributeString("ToolsVersion", "4.0");
					xmlWriter.WriteAttributeString("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003");
					xmlWriter.WriteStartElement("PropertyGroup");
					foreach (PropertyInfo pi in properties)
					{
						xmlWriter.WriteElementString(pi.Name, pi.GetValue(this, null)?.ToString() ?? string.Empty);
					}
					xmlWriter.WriteEndElement();
					xmlWriter.WriteEndElement();
					xmlWriter.WriteEndDocument();
					// TODO: Logic to handle itemgroups
					msDeployPublishXmlContents = stringWriter.ToString();
				}
			}

			return msDeployPublishXmlContents;
		}

		public void LoadModel(string pubxmlContents)
		{
			using (XmlTextReader reader = new XmlTextReader(new StringReader(pubxmlContents)))
			{
				Type t = GetType();
				var properties = t.GetProperties()
				   .Where(prop => prop.PropertyType == typeof(string) || prop.PropertyType == typeof(bool))
				   .OrderBy(p => p.GetCustomAttributes(typeof(DisplayAttribute), true)
				   .Cast<DisplayAttribute>()
				   .Select(a => a.Order)
				   .FirstOrDefault());
				Project project = new Project(reader);
				foreach (PropertyInfo pi in properties)
				{
					ProjectProperty msbuildProperty = project.AllEvaluatedProperties.FirstOrDefault(prop => string.Equals(prop.Name, pi.Name, StringComparison.OrdinalIgnoreCase));
					if (msbuildProperty != null)
					{
						object propertyValue = msbuildProperty.EvaluatedValue;
						if (pi.PropertyType == typeof(bool))
						{
							propertyValue = Convert.ChangeType(propertyValue, TypeCode.Boolean);
						}

						pi.SetValue(this, propertyValue);
					}
				}
			}
		}
	}
}