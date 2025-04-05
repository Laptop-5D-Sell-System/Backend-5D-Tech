using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.Services
{
    public class CloudianaryService
    {
        private readonly Cloudinary _cloudianary;
        public CloudianaryService()
        {
            var acc = new Account(
                System.Configuration.ConfigurationManager.AppSettings["CloudName"],
                System.Configuration.ConfigurationManager.AppSettings["APIKey"],
                System.Configuration.ConfigurationManager.AppSettings["APISecret"]
            );
            _cloudianary = new Cloudinary(acc);
        }

        public string UploadImage(HttpPostedFile file)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new Exception("File không hợp lệ");
            }

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.InputStream),
                Transformation = new Transformation().Quality(80).Crop("limit").Width(800).Height(800)
            };
            var result = _cloudianary.Upload(uploadParams);
            return result.SecureUrl.ToString();
        }

    }
}