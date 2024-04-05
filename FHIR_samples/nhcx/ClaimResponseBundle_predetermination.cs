using System;
using Hl7.Fhir.Model;
using System.Collections.Generic;
using FHIR_NHCX;
namespace NHCX_Sample_code
{
    class ClaimResponseBundle_predetermination
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside ClaimResponseBundle_predetermination");
                fnClaimResponseBundle_predetermination(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("ClaimResponseBundle_predetermination ERROR:---" + e.Message);
            }

        }

        static bool fnClaimResponseBundle_predetermination(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle ClaimResponseBundle_predetermination = new Bundle();
                ClaimResponseBundle_predetermination = populateClaimResponseBundle_predetermination();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(ClaimResponseBundle_predetermination, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated ClaimResponseBundle_predetermination bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("ClaimResponseBundle_predetermination.json", ClaimResponseBundle_predetermination);
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

        static Bundle populateClaimResponseBundle_predetermination()
        {
            // Set metadata about the resource            
            Bundle ClaimResponseBundle_predetermination = new Bundle()
            {
                // Set logical id of this artifact
                Id = "ClaimResponseBundle-predetermination-01",
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/ClaimResponseBundle",
                    },
                    // Set Confidentiality as defined by affinity domain
                    Security = new List<Coding>()
                    {
                        new Coding("http://terminology.hl7.org/CodeSystem/v3-Confidentiality", "V", "very restricted"),
                    }
                },
            };

            // Set Bundle Type 
            ClaimResponseBundle_predetermination.Type = Bundle.BundleType.Collection;

            // Set version-independent identifier for the Bundle
            Identifier identifier = new Identifier();
            identifier.Value = "bc3c6c57-2053-4d0e-ac40-139ccccff645";
            identifier.System = "http://hip.in";
            ClaimResponseBundle_predetermination.Identifier = identifier;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            ClaimResponseBundle_predetermination.TimestampElement = new Instant(DateTime.Parse(dtStr));


            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:731b668b-d5b2-43a3-923f-20e284113919";                                   //ClaimResponse/ClaimResponse-predetermination-01;
            bundleEntry1.Resource = ResourcePopulator.populateClaimResponsepredetermination();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:372a5471-1e67-4501-8c29-b20b783ba33e";
            bundleEntry2.Resource = ResourcePopulator.populateClaimpredeterminationResource();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry2);
            
            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";
            bundleEntry3.Resource = ResourcePopulator.populatePatientResource();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry4.Resource = ResourcePopulator.populateOrganizationResource();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";
            bundleEntry5.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry5);

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9";         // MedicationRequest/MedicationRequest-01;
            bundleEntry6.Resource = ResourcePopulator.populateSecondMedicationRequestResource();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";
            bundleEntry7.Resource = ResourcePopulator.populatePractitionerResource();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";
            bundleEntry8.Resource = ResourcePopulator.populateCoverageResource();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry8);

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4";
            bundleEntry9.Resource = ResourcePopulator.populateConditionResource();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry9);

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e";
            bundleEntry10.Resource = ResourcePopulator.populateDocumentReferenceResource();
            ClaimResponseBundle_predetermination.Entry.Add(bundleEntry10);

            return ClaimResponseBundle_predetermination;
        }
    }
}
