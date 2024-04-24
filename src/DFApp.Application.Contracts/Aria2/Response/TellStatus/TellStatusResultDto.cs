using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace DFApp.Aria2.Response.TellStatus
{
    public class TellStatusResultDto: CreationAuditedEntityDto<long>
    {
        /// <summary>
        /// 
        /// </summary>
        public string bitfield { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string completedLength { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string connections { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string dir { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string downloadSpeed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string errorCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string errorMessage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<FilesItemDto> files { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string gid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string numPieces { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pieceLength { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string totalLength { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uploadLength { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uploadSpeed { get; set; }
    }
}
