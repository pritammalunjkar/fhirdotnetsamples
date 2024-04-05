using FHIR_NHCX;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHCX_Sample_code
{
    class ClaimBundleResource_settlement
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside ClaimBundleResource_settlement");
                fnClaimBundleResource_settlement(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("ClaimBundleResource_settlement ERROR:---" + e.Message);
            }

        }
        static bool fnClaimBundleResource_settlement(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle ClaimBundleResource_settlement = new Bundle();
                ClaimBundleResource_settlement = populateClaimBundleResource_settlement();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(ClaimBundleResource_settlement, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated ClaimBundleResource_settlement bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("ClaimBundleResource_settlement.json", ClaimBundleResource_settlement);
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
        static Bundle populateClaimBundleResource_settlement()
        {
            // Set metadata about the resource            
            Bundle ClaimBundleResource_settlement = new Bundle()
            {
                // Set logical id of this artifact
                Id = "ClaimBundle-settlement-01",    // ClaimBundle-settlement-01",
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
            ClaimBundleResource_settlement.Identifier = identifier;

            // Set Bundle Type 
            ClaimBundleResource_settlement.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2023-12-13T15:32:26.605+05:30";
            ClaimBundleResource_settlement.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:4776dbdf-d596-4cd1-9966-9d44ae9dec0b";   // Claim/Claim-settlement-01;           
            bundleEntry1.Resource = ResourcePopulator.populateClaimsettlement();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";
            bundleEntry2.Resource = ResourcePopulator.populatePatientResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry3.Resource = ResourcePopulator.populateOrganizationResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry3);


            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";
            bundleEntry4.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9";
            bundleEntry5.Resource = ResourcePopulator.populateSecondMedicationRequestResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry5);
            
            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";
            bundleEntry6.Resource = ResourcePopulator.populatePractitionerResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";
            bundleEntry7.Resource = ResourcePopulator.populateCoverageResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4";
            bundleEntry8.Resource = ResourcePopulator.populateConditionResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry8);

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:7aace234-5172-4126-a907-ace8745bd1a5";           
            bundleEntry9.Resource = ResourcePopulator.populateClaimenhancementResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry9);

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:760ec93e-7ec8-4e82-b8a3-fe6512fccd8b";
            bundleEntry10.Resource = ResourcePopulator.populateClaimpreauthResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry10);

            var bundleEntry11 = new Bundle.EntryComponent();
            bundleEntry11.FullUrl = "urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e";
            bundleEntry11.Resource = ResourcePopulator.populateDocumentReferenceResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry11);

            var bundleEntry12 = new Bundle.EntryComponent();
            bundleEntry12.FullUrl = "urn:uuid:514bcad3-7bf0-43a0-b566-e8ecd815dc91";
            bundleEntry12.Resource = ResourcePopulator.populateSecondDocumentReferenceResource();
            ClaimBundleResource_settlement.Entry.Add(bundleEntry12);

            return ClaimBundleResource_settlement;
        }
    }
}
