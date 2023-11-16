using CommonControls.Interfaces.AssetManagement;
using Microsoft.Extensions.DependencyInjection;
using CommonControls;
using AssetManagement.Geometry.Strategies.Fbx.FbxAssetHandling;
using AssetManagement.Geometry.Services;
using CommonControls.Events.UiCommands;

namespace AssetManagement
{
    public class AssetManagement_DependencyInjectionContainer : DependencyContainer
    {
        public override void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAssetImporterProvider, AssetImporterProvider>();
            serviceCollection.AddTransient<IAssetImporter, FbxAssetImporter>();

            serviceCollection.AddScoped<IAssetExporterProvider, AssetExporterProvider>();
            serviceCollection.AddTransient<IAssetExporter, FbxAssetExporter>();


            // make these commands happen in "assetEditor"
            serviceCollection.AddTransient<ImportAssetCommand>();
            serviceCollection.AddTransient<ExportAssetCommand>(); 


        }
    }
}
