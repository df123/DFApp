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
        private IConfigurationInfoRepository _configurationRepository;

        public ConfigurationInfoDataSeederContributor(IConfigurationInfoRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if ((await _configurationRepository.GetCountAsync()) <= 0)
            {
                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = string.Empty,
                    ConfigurationName = "ReturnDownloadUrlPrefix",
                    ConfigurationValue = "apache或者nginx服务器配置的下载地址",
                    Remark = string.Empty
                }, autoSave: true);

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = string.Empty,
                    ConfigurationName = "SavePhotoPathPrefix",
                    ConfigurationValue = "./Photo",
                    Remark = string.Empty
                }, autoSave: true);

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = string.Empty,
                    ConfigurationName = "SaveVideoPathPrefix",
                    ConfigurationValue = "./Video",
                    Remark = string.Empty
                }, autoSave: true);

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Background.ListenTelegramService",
                    ConfigurationName = "session_pathname",
                    ConfigurationValue = "./WTelegram.session",
                    Remark = string.Empty
                });

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Background.ListenTelegramService",
                    ConfigurationName = "api_id",
                    ConfigurationValue = "请输入api_id",
                    Remark = string.Empty
                });

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Background.ListenTelegramService",
                    ConfigurationName = "api_hash",
                    ConfigurationValue = "请输入api_hash",
                    Remark = string.Empty
                });

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Background.ListenTelegramService",
                    ConfigurationName = "phone_number",
                    ConfigurationValue = "请输入phone_number",
                    Remark = string.Empty
                });

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = string.Empty,
                    ConfigurationName = "ProxyHost",
                    ConfigurationValue = "127.0.0.1",
                    Remark = string.Empty
                }, autoSave: true);

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = string.Empty,
                    ConfigurationName = "ProxyPort",
                    ConfigurationValue = "10079",
                    Remark = string.Empty
                }, autoSave: true);

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = string.Empty,
                    ConfigurationName = "EnableProxy",
                    ConfigurationValue = "false",
                    Remark = string.Empty
                }, autoSave: true);

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Background.MediaBackgroudService",
                    ConfigurationName = "ZipType",
                    ConfigurationValue = "JPG",
                    Remark = string.Empty
                }, autoSave: true);

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Background.MediaBackgroudService",
                    ConfigurationName = "ReplaceUrlPrefix",
                    ConfigurationValue = "./",
                    Remark = string.Empty
                }, autoSave: true);

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Controllers.FileUpDownloadController.ContentType",
                    ConfigurationName = ".drawio",
                    ConfigurationValue = "application/xml",
                    Remark = string.Empty
                }, autoSave: true);

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Controllers.FileUpDownloadController.ContentType",
                    ConfigurationName = ".iso",
                    ConfigurationValue = "application/octet-stream",
                    Remark = string.Empty
                }, autoSave: true);


                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Aria2.Aria2Service",
                    ConfigurationName = "replaceString",
                    ConfigurationValue = "Aria2下载文件保存的位置",
                    Remark = string.Empty
                });

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Aria2.Aria2Service",
                    ConfigurationName = "aria2secret",
                    ConfigurationValue = "Aria2设置的连接密钥",
                    Remark = string.Empty
                });

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = "DFApp.Aria2.Aria2Service",
                    ConfigurationName = "Aria2BtDownloadUrlPrefix",
                    ConfigurationValue = "apache或者nginx服务器配置的下载地址",
                    Remark = string.Empty
                });

                await _configurationRepository.InsertAsync(new ConfigurationInfo
                {
                    ModuleName = string.Empty,
                    ConfigurationName = "aria2ws",
                    ConfigurationValue = "ws://127.0.01:6800/jsonrpc",
                    Remark = "aria2的ws连接地址"
                });

            }
        }
    }
}
