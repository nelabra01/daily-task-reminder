using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Text;


namespace BlobStorageAccount
{
    internal class Program
    {
        private static readonly string _kegOrderS3Bucket = "ne-bucket-1";
        private static readonly string _accountStorageUrl;
        private static readonly BlobServiceClient _blobServiceClient;


        static Program()
        {
            _accountStorageUrl = ConfigurationManager.AppSettings["StorageBlobAccountUrl"];
            if (_accountStorageUrl == null)
                throw new ArgumentNullException("StorageBlobAccountUrl");

            _blobServiceClient = new BlobServiceClient(new Uri(_accountStorageUrl), new DefaultAzureCredential());
        }

        static void Main(string[] args)
        {
            //var success = UploadContent();
            var url = DownloadFile();
        }

        private static string DownloadFile()
        {
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("ne-bucket-1");

            BlobClient blobClient = blobContainerClient.GetBlobClient("food-org.jpg");
            var imgStream = blobClient.OpenRead();

            using (var img = new Bitmap(imgStream))
            {
                using(var imgScaled = new Bitmap(img, 500, 300))
                {
                    using(var imgScaledStream = new MemoryStream())
                    {
                        imgScaled.Save(imgScaledStream, ImageFormat.Jpeg);
                        blobClient = blobContainerClient.GetBlobClient($"{Guid.NewGuid()}/food-scaled.jpg");
                        imgScaledStream.Position = 0;
                        blobClient.Upload(imgScaledStream,
                            httpHeaders: new BlobHttpHeaders 
                            {
                                ContentType = "image/jpeg",
                                CacheControl = "max-age=604800"
                            });
                        return blobClient.Uri.ToString();
                    }
                }
            }
        }

        private static string GetSkuPrimaryImageUrl(Guid skuId, Guid companyId, int? width = null, int? height = null, bool forceRegeneration = false)
        {
            string croppedPhotoId = $"{skuId}_{width}x{height}.jpg";
            string croppedPhotoKey = $"public/cache/SkuPhoto/{croppedPhotoId}";

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_kegOrderS3Bucket);

            var blobClient = blobContainerClient.GetBlobClient(croppedPhotoKey);

            if (blobClient.Exists() && !forceRegeneration)
            {
                return blobClient.Uri.ToString();
            }

            string originalPhotoKey = $"SkuPhotos/{skuId}.jpg";

            blobClient = blobContainerClient.GetBlobClient(originalPhotoKey);

            if (blobClient.Exists())
            {
                var scaledImageUrl = CreateScaledVersionInS3(blobClient.OpenRead(), croppedPhotoKey, width, height);
                return scaledImageUrl;
            }

            return null;
        }

        private string GetCompanyLogo(Guid companyId, int? width, int? height)
        {
            string croppedPhotoId = $"{companyId}_{width}x{height}.jpg";
            string croppedPhotoKey = $"public/cache/CompanyLogo/{croppedPhotoId}";

            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(_kegOrderS3Bucket);
            BlobClient blobClient = blobContainerClient.GetBlobClient(croppedPhotoKey);

            if(blobClient.Exists())
            {
                return blobClient.Uri.ToString();
            }

            string originalPhotoKey = $"CompanyLogo/{companyId}.jpg";

            blobClient = blobContainerClient.GetBlobClient(originalPhotoKey);
            
            if(blobClient.Exists())
            {
                var scaledImageUrl = CreateScaledVersionInS3(blobClient.OpenRead(), croppedPhotoKey, width, height);
                return scaledImageUrl;
            }

            return null;
        }

        public static string CreateScaledVersionInS3(Stream originalPhotoStream, string croppedPhotoKey, int? width = null, int? height = null)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_kegOrderS3Bucket);
            try
            {
                using (var imgOriginal = new Bitmap(originalPhotoStream))
                {
                    using (var imgScaled = new Bitmap(imgOriginal, width ?? imgOriginal.Width, height ?? imgOriginal.Width))
                    {
                        using (var imgScaledStream = new MemoryStream())
                        {
                            imgScaled.Save(imgScaledStream, ImageFormat.Jpeg);
                            var blobClient = blobContainerClient.GetBlobClient(croppedPhotoKey);
                            imgScaledStream.Position = 0;
                            blobClient.Upload(imgScaledStream,
                                httpHeaders: new BlobHttpHeaders() 
                                {
                                    ContentType = "image/jpeg",
                                    CacheControl = "max-age=604800"
                                });
                            return blobClient.Uri.ToString();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static bool UploadContent()
        {
            var client = new BlobServiceClient(new Uri("https://neblobstoragetestaccount.blob.core.windows.net/"), new DefaultAzureCredential());
            BlobContainerClient blobContainerClient = client.GetBlobContainerClient("ne-bucket-1");
            try
            {
                using (var contentStream = new MemoryStream(Encoding.Unicode.GetBytes(dummyText)))
                {
                    var key = Guid.NewGuid();
                    BlobClient blobClient = blobContainerClient.GetBlobClient($"{key}.json");
                    var response = blobClient.Upload(contentStream);
                    return !response?.GetRawResponse()?.IsError ?? false;
                }
            }
            catch
            {
                return false;
            }            
        }

        static string dummyText = @"[
  {
    ""_id"": ""6719068f2a52757719527cbb"",
    ""index"": 0,
    ""guid"": ""b4c3e1ea-1b4f-4cde-9af7-6d2cbef38758"",
    ""isActive"": false,
    ""balance"": ""$2,629.11"",
    ""picture"": ""http://placehold.it/32x32"",
    ""age"": 23,
    ""eyeColor"": ""green"",
    ""name"": ""June Pearson"",
    ""gender"": ""female"",
    ""company"": ""VORATAK"",
    ""email"": ""junepearson@voratak.com"",
    ""phone"": ""+1 (866) 476-3006"",
    ""address"": ""887 Hill Street, Orason, North Dakota, 8329"",
    ""about"": ""Duis aute duis dolor aliqua ex ipsum cupidatat veniam commodo. Enim deserunt irure nostrud mollit nostrud. Sint pariatur eiusmod dolor adipisicing nulla consectetur consectetur. Sunt aliquip aliqua veniam et eu ad magna tempor proident.\r\n"",
    ""registered"": ""2018-12-15T06:52:37 +05:00"",
    ""latitude"": 0.905497,
    ""longitude"": 153.600931,
    ""tags"": [
      ""minim"",
      ""exercitation"",
      ""dolor"",
      ""eu"",
      ""adipisicing"",
      ""culpa"",
      ""ex""
    ],
    ""friends"": [
      {
        ""id"": 0,
        ""name"": ""Lott Figueroa""
      },
      {
        ""id"": 1,
        ""name"": ""Holden Richard""
      },
      {
        ""id"": 2,
        ""name"": ""Mendoza Mayo""
      }
    ],
    ""greeting"": ""Hello, June Pearson! You have 9 unread messages."",
    ""favoriteFruit"": ""apple""
  },
  {
    ""_id"": ""6719068feb3624002d124c17"",
    ""index"": 1,
    ""guid"": ""475b7ac8-5655-4230-bc90-3688f6970446"",
    ""isActive"": false,
    ""balance"": ""$3,986.97"",
    ""picture"": ""http://placehold.it/32x32"",
    ""age"": 40,
    ""eyeColor"": ""green"",
    ""name"": ""Frankie Marsh"",
    ""gender"": ""female"",
    ""company"": ""FARMAGE"",
    ""email"": ""frankiemarsh@farmage.com"",
    ""phone"": ""+1 (859) 508-3889"",
    ""address"": ""438 Overbaugh Place, Haring, Rhode Island, 8803"",
    ""about"": ""Ipsum consequat duis nulla elit cupidatat voluptate sunt Lorem ut sit. Velit labore eiusmod est ad aliqua magna in mollit tempor. Occaecat culpa dolor dolor dolore. Est irure excepteur aliqua nulla ipsum nulla aute sit. Non id ea ea adipisicing duis ullamco ut dolore ullamco Lorem. Laborum elit magna deserunt ea excepteur enim. Et tempor ullamco minim nisi anim veniam laboris esse sunt.\r\n"",
    ""registered"": ""2017-08-14T10:36:02 +04:00"",
    ""latitude"": -86.146381,
    ""longitude"": -101.283029,
    ""tags"": [
      ""ut"",
      ""sint"",
      ""non"",
      ""laboris"",
      ""sint"",
      ""sit"",
      ""eiusmod""
    ],
    ""friends"": [
      {
        ""id"": 0,
        ""name"": ""Schwartz Hubbard""
      },
      {
        ""id"": 1,
        ""name"": ""Parker Nicholson""
      },
      {
        ""id"": 2,
        ""name"": ""Gibbs Vaughan""
      }
    ],
    ""greeting"": ""Hello, Frankie Marsh! You have 10 unread messages."",
    ""favoriteFruit"": ""strawberry""
  },
  {
    ""_id"": ""6719068fda1f23b47c5f19d9"",
    ""index"": 2,
    ""guid"": ""a6d5eb17-3551-49f7-8278-2bfee8ec2612"",
    ""isActive"": true,
    ""balance"": ""$3,661.26"",
    ""picture"": ""http://placehold.it/32x32"",
    ""age"": 27,
    ""eyeColor"": ""brown"",
    ""name"": ""Johnnie Mosley"",
    ""gender"": ""female"",
    ""company"": ""ZILLIDIUM"",
    ""email"": ""johnniemosley@zillidium.com"",
    ""phone"": ""+1 (885) 437-3043"",
    ""address"": ""517 Seeley Street, Ferney, Minnesota, 6652"",
    ""about"": ""Qui enim occaecat elit eiusmod. Sint dolore veniam ea sunt ut aliquip mollit ipsum consectetur. Ea laborum ex elit culpa duis consectetur ipsum.\r\n"",
    ""registered"": ""2018-12-21T12:41:58 +05:00"",
    ""latitude"": -62.653939,
    ""longitude"": 10.318343,
    ""tags"": [
      ""officia"",
      ""veniam"",
      ""aliquip"",
      ""proident"",
      ""cupidatat"",
      ""cillum"",
      ""ullamco""
    ],
    ""friends"": [
      {
        ""id"": 0,
        ""name"": ""Olivia Schwartz""
      },
      {
        ""id"": 1,
        ""name"": ""Shields Contreras""
      },
      {
        ""id"": 2,
        ""name"": ""Gladys Velasquez""
      }
    ],
    ""greeting"": ""Hello, Johnnie Mosley! You have 5 unread messages."",
    ""favoriteFruit"": ""apple""
  },
  {
    ""_id"": ""6719068f7485c4982273afea"",
    ""index"": 3,
    ""guid"": ""6d779928-1bd8-435a-8861-771ad6103543"",
    ""isActive"": true,
    ""balance"": ""$2,835.89"",
    ""picture"": ""http://placehold.it/32x32"",
    ""age"": 31,
    ""eyeColor"": ""brown"",
    ""name"": ""Virginia Green"",
    ""gender"": ""female"",
    ""company"": ""ISOSTREAM"",
    ""email"": ""virginiagreen@isostream.com"",
    ""phone"": ""+1 (872) 565-3177"",
    ""address"": ""334 Nolans Lane, Sunwest, Palau, 6408"",
    ""about"": ""Aliquip nulla sint ea aliquip non officia. Nisi exercitation duis consectetur incididunt sit cillum in consequat sint pariatur officia sunt id. Irure amet ut occaecat laboris anim sit Lorem laboris esse qui irure do incididunt ullamco. Tempor veniam exercitation labore cupidatat dolore amet irure. Mollit mollit ut est adipisicing dolor ut commodo aliquip nulla aliquip ex sit ad velit. Dolor proident ad exercitation est deserunt nostrud ullamco. Nulla laboris non exercitation ipsum sunt excepteur consectetur tempor.\r\n"",
    ""registered"": ""2021-03-23T03:32:42 +04:00"",
    ""latitude"": -60.163314,
    ""longitude"": 1.693139,
    ""tags"": [
      ""est"",
      ""nostrud"",
      ""laboris"",
      ""irure"",
      ""deserunt"",
      ""est"",
      ""proident""
    ],
    ""friends"": [
      {
        ""id"": 0,
        ""name"": ""Camille Bishop""
      },
      {
        ""id"": 1,
        ""name"": ""Jaclyn Fox""
      },
      {
        ""id"": 2,
        ""name"": ""Briana Hoffman""
      }
    ],
    ""greeting"": ""Hello, Virginia Green! You have 10 unread messages."",
    ""favoriteFruit"": ""strawberry""
  },
  {
    ""_id"": ""6719068f45462e2ef24fabae"",
    ""index"": 4,
    ""guid"": ""f49d2275-e746-4288-8720-d5b6c40d8b2e"",
    ""isActive"": true,
    ""balance"": ""$1,419.30"",
    ""picture"": ""http://placehold.it/32x32"",
    ""age"": 27,
    ""eyeColor"": ""brown"",
    ""name"": ""Wright Molina"",
    ""gender"": ""male"",
    ""company"": ""AEORA"",
    ""email"": ""wrightmolina@aeora.com"",
    ""phone"": ""+1 (896) 466-3792"",
    ""address"": ""177 Berriman Street, Rosewood, New Mexico, 6801"",
    ""about"": ""Est pariatur proident anim id ipsum labore. In reprehenderit exercitation adipisicing id amet in voluptate officia. Laboris deserunt nulla nostrud Lorem enim commodo ad ullamco dolore dolore consectetur. Mollit proident enim deserunt ad pariatur commodo voluptate. Irure mollit officia officia Lorem culpa est nulla nisi voluptate pariatur anim adipisicing. Culpa incididunt officia non sint.\r\n"",
    ""registered"": ""2023-10-27T06:07:33 +04:00"",
    ""latitude"": 45.078255,
    ""longitude"": 26.187875,
    ""tags"": [
      ""aliquip"",
      ""est"",
      ""non"",
      ""deserunt"",
      ""qui"",
      ""ipsum"",
      ""esse""
    ],
    ""friends"": [
      {
        ""id"": 0,
        ""name"": ""Natalie Grant""
      },
      {
        ""id"": 1,
        ""name"": ""Haley Bolton""
      },
      {
        ""id"": 2,
        ""name"": ""Owen Odom""
      }
    ],
    ""greeting"": ""Hello, Wright Molina! You have 9 unread messages."",
    ""favoriteFruit"": ""strawberry""
  }
]";
    }
}
