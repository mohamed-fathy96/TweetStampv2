using System.IO;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.policy;
using tr.gov.tubitak.uekae.esya.api.common.util;

namespace TweetStampv2.m3api.tr_TR
{
    public static class TestConstants
    {
        static TestConstants()
        {
            var LICENCE_FILE = Path.Combine("m3api", "lisans", "lisans.xml");
            setLicence(LICENCE_FILE);
        }

        public static void setLicence(string LICENCE_FILE)
        {
            using (Stream license = new FileStream(LICENCE_FILE, FileMode.Open, FileAccess.Read))
            {
                LicenseUtil.setLicenseXml(license);
            }
        }
        public static ValidationPolicy GetPolicy(string POLICY_FILE)
        {
            string dir = Directory.GetCurrentDirectory();           
            return PolicyReader.readValidationPolicy(new FileStream(POLICY_FILE, FileMode.Open, FileAccess.Read));
        }
    }
}
