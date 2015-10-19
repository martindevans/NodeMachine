using System.IO;
using Base_CityGeneration.Elements.Building;
using Base_CityGeneration.Elements.Building.Design;
using EpimetheusPlugins.Scripts;

namespace /*TEMPLATED_NAMESPACE*/
{
    [Script("/*TEMPLATED_BUILDING_GUID*/", "/*TEMPLATED_BUILDING_DESCRIPTION*/")]
    public class BuildingTemplate_/*TEMPLATED_BUILDING_NAME*/
        : SpecBuildingContainer /*TEMPLATED_BUILDING_TAGS*/
    {
        public BuildingTemplate_/*TEMPLATED_BUILDING_NAME*/()
            : base(BuildingDesigner.Deserialize(new StringReader(@"/*TEMPLATED_BUILDING_SCRIPT*/")))
        {
        }
    }
}
