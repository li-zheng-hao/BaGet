using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BaGet
{
    public static class CerMaker
    {
        // 先创建自签名证书
        public static string CER_FILE = "host.pfx";
        public static string PASSWD = "admin123";
        public static string SUB_NAME = "CN=baget";

        public static void CreateIfNotExist()
        {
            DateTime today = DateTime.Now;
            DateTime endday = today.AddDays(365);
            if(!File.Exists(CER_FILE))
            {
                Console.WriteLine($"生成证书参数： {CER_FILE} {PASSWD}");
                CreateSslCertAsync(SUB_NAME, today, endday, CER_FILE, PASSWD).GetAwaiter().GetResult();
            }
        }

        public static async Task CreateSslCertAsync(string subName,
            DateTime bgDate,
            DateTime endDate,
            string outFile,
            string? passWd)
        {
            // 参数检查
            if(subName is null or { Length: < 3 })
            {
                throw new ArgumentNullException(nameof(subName));
            }
            if(endDate <= bgDate)
            {
                throw new ArgumentException("结束日期应大于开始日期");
            }
            // 随机密钥
            RSA key = RSA.Create(1024);
            // 创建CRT
            CertificateRequest crt = new(subName, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            // 创建自签名证书
            var cert = crt.CreateSelfSigned(bgDate, endDate);
            // 将证书写入文件
            byte[] data = cert.Export(X509ContentType.Pfx, passWd);
            await File.WriteAllBytesAsync(outFile, data);
        }
    }
}
