using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace DFApp.Configuration
{
    public class ConfigurationInfoDataSeederContributor : IDataSeedContributor, ITransientDependency
    {
        private IConfigurationInfoRepository _configurationInfoRepository;

        public ConfigurationInfoDataSeederContributor(IConfigurationInfoRepository configurationInfoRepository)
        {
            _configurationInfoRepository = configurationInfoRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _configurationInfoRepository.GetCountAsync() <= 0)
            {
                await _configurationInfoRepository.InsertAsync(new ConfigurationInfo()
                {
                    ModuleName = "DFApp.Controllers.FileUpDownloadController",
                    ConfigurationName = "SaveUplouadFilePath",
                    ConfigurationValue = "./",
                    Remark = "设置文件传输上传文件的保存路径"
                },
                autoSave: true);

                await _configurationInfoRepository.InsertAsync(new ConfigurationInfo()
                {
                    ModuleName = "DFApp.Media.MediaInfoService",
                    ConfigurationName = "SavePhotoPathPrefix",
                    ConfigurationValue = "./",
                    Remark = "设置媒体图片保存位置"
                },
                autoSave: true);

                await _configurationInfoRepository.InsertAsync(new ConfigurationInfo()
                {
                    ModuleName = "DFApp.Media.MediaInfoService",
                    ConfigurationName = "ReturnDownloadUrlPrefix",
                    ConfigurationValue = "https://example.com",
                    Remark = "设置媒体HTTP文件服务器地址"
                },
                autoSave: true);

            }
        }
    }
}
