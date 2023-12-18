using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using Salient.WeatherStack.Activities.Design.Designers;
using Salient.WeatherStack.Activities.Design.Properties;

namespace Salient.WeatherStack.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(GetWeather), categoryAttribute);
            builder.AddCustomAttributes(typeof(GetWeather), new DesignerAttribute(typeof(GetWeatherDesigner)));
            builder.AddCustomAttributes(typeof(GetWeather), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
