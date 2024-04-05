using FHIR_NHCX;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHCX_Sample_code
{
    class ClaimBundleResource_predetermination
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside ClaimBundleResource_predetermination");
                fnClaimBundleResource_predetermination(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("ClaimBundleResource_predetermination ERROR:---" + e.Message);
            }

        }
        static bool fnClaimBundleResource_predetermination(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle ClaimBundleResource_predetermination = new Bundle();
                ClaimBundleResource_predetermination = populateClaimBundleResource_predetermination();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(ClaimBundleResource_predetermination, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated ClaimBundleResource_predetermination bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("ClaimBundleResource_predetermination.json", ClaimBundleResource_predetermination);
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

        // populating ClaimBundle Resource
        static Bundle populateClaimBundleResource_predetermination()
        {
            // Set metadata about the resource            
            Bundle ClaimBundleResource_predetermination = new Bundle()
            {
                // Set logical id of this artifact
                Id = "bc3c6c57-2053-4d0e-ac40-139ccccff645", // "ClaimBundle-predetermination-01",
                
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ClaimBundle",
                    },
                    // Set Confidentiality as defined by affinity domain
                    Security = new List<Coding>()
                    {
                        new Coding("http://terminology.hl7.org/CodeSystem/v3-Confidentiality", "V", "very restricted"),
                    }
                },
            };


            // Set version-independent identifier for the Bundle
            Identifier identifier = new Identifier();
            identifier.Value = "bc3c6c57-2053-4d0e-ac40-139ccccff645";
            identifier.System = "http://hip.in";
            ClaimBundleResource_predetermination.Identifier = identifier;

            // Set Bundle Type 
            ClaimBundleResource_predetermination.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2023-12-13T15:32:26.605+05:30";
            ClaimBundleResource_predetermination.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:372a5471-1e67-4501-8c29-b20b783ba33e";   // "Claim/Claim-predetermination-01"; 
            bundleEntry1.Resource = ResourcePopulator.populateClaimpredeterminationResource();
            ClaimBundleResource_predetermination.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";
            bundleEntry2.Resource = ResourcePopulator.populatePatientResource();
            ClaimBundleResource_predetermination.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry3.Resource = ResourcePopulator.populateOrganizationResource();
            ClaimBundleResource_predetermination.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";
            bundleEntry4.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            ClaimBundleResource_predetermination.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9";
            bundleEntry5.Resource = ResourcePopulator.populateSecondMedicationRequestResource();
            ClaimBundleResource_predetermination.Entry.Add(bundleEntry5);

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";
            bundleEntry6.Resource = ResourcePopulator.populatePractitionerResource();
            ClaimBundleResource_predetermination.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";
            bundleEntry7.Resource = ResourcePopulator.populateCoverageResource();
            ClaimBundleResource_predetermination.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4";
            bundleEntry8.Resource = ResourcePopulator.populateConditionResource();
            ClaimBundleResource_predetermination.Entry.Add(bundleEntry8);

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e";
            bundleEntry9.Resource = ResourcePopulator.populateDocumentReferenceResource();
            ClaimBundleResource_predetermination.Entry.Add(bundleEntry9);

            return ClaimBundleResource_predetermination;
        }
    }
}
