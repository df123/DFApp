using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace DFApp.Aria2.Response.TellStatus
{
    public class TellStatusResultDto: CreationAuditedEntityDto<long>
    {
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("bitfield")]
        public string Bitfield { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("completedLength")]
        public string CompletedLength { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("connections")]
        public string Connections { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("dir")]
        public string Dir { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("downloadSpeed")]
        public string DownloadSpeed { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("files")]
        public List<FilesItemDto> Files { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("gid")]
        public string Gid { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("numPieces")]
        public string NumPieces { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("pieceLength")]
        public string PieceLength { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("totalLength")]
        public string TotalLength { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("uploadLength")]
        public string UploadLength { get; set; }
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("uploadSpeed")]
        public string UploadSpeed { get; set; }
    }
}
