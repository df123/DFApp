using DFApp.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace DFApp.Media
{
    public class MediaInfoDataSeederContributor : IDataSeedContributor, ITransientDependency
    {
        private IMediaRepository _mediaRepository;

        public MediaInfoDataSeederContributor(IMediaRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _mediaRepository.GetCountAsync() <= 0)
            {
                List<MediaInfo> lms = new List<MediaInfo>();

                string folderPath = @"D:\Users\jack_huang\GitHubProject\CopyAndZip"; // 指定文件夹路径

                string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

                int index = 0;
                foreach (string file in files)
                {
                    if (file.Contains("bin") || file.Contains("obj") || file.Contains(".github") 
                        || file.Contains("node_modules") || file.Contains("node_modules")
                        || file.Contains("wwwroot"))
                    {
                        continue;
                    }

                    lms.Add(new MediaInfo
                    {
                        AccessHash = index++,
                        TID = index++,
                        Size = index++,
                        SavePath = file,
                        ValueSHA1 = HashHelper.CalculationHash(file),
                        MimeType = ".jpg"
                    });
                }

                await _mediaRepository.InsertManyAsync(lms,autoSave: true);

            }
        }
    }
}
