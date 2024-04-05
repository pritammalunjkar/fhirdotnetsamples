using System;
using Hl7.Fhir.Model;
using System.Collections.Generic;
using FHIR_NHCX;

namespace NHCX_Sample_code
{
    class InsurancePlanBundle
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside InsurancePlanBundle");                
                fnInsurancePlanBundle(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("InsurancePlanBundle ERROR:---" + e.Message);
            }

        }

        static bool fnInsurancePlanBundle(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle InsurancePlanBundle = new Bundle();
                InsurancePlanBundle = populateInsurancePlanBundle();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(InsurancePlanBundle, ref strErr_OUT);               
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated InsurancePlanBundle bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("InsurancePlanBundle.json", InsurancePlanBundle);
                    if (isProfileCreated == false)
                    {
                        Console.WriteLine("Error in Profile File creation");
                    }
                    else
                    {
                        Console.WriteLine("Success");
                    }
                }
                strError_OUT = "";
                return blnReturn;
            }
            catch (Exception ex)
            {
                blnReturn = false;
                strError_OUT = ex.InnerException.ToString();
                return blnReturn;
            }
        }

        static Bundle populateInsurancePlanBundle()
        {
            // Set metadata about the resource            
            Bundle insurancePlanBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "InsuarncePlanBundle-01",
                Meta = new Meta()
                {
                    VersionId = "1",                   
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/InsurancePlanBundle",
                    },
                    // Set Confidentiality as defined by affinity domain
                    Security = new List<Coding>()
                    {
                        new Coding("http://terminology.hl7.org/CodeSystem/v3-Confidentiality", "V", "very restricted"),
                    }
                },
            };

            // Set Bundle Type 
            insurancePlanBundle.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            insurancePlanBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:859d47c1-fd1a-45e4-b725-22e0a1e0b84c";           //InsurancePlan/InsurancePlan-01;
            bundleEntry1.Resource = ResourcePopulator.populateInsurancePlan();
            insurancePlanBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry2.Resource = ResourcePopulator.populateOrganizationResource();
            insurancePlanBundle.Entry.Add(bundleEntry2);

            return insurancePlanBundle;
        }


        
    }
}
