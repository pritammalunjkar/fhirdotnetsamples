using FHIR_NHCX;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;


namespace NHCX_Sample_code
{
    class TaskBundleForSearchResponse
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside TaskBundleForSearchResponse");
                fnTaskBundleForSearchResponse(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("TaskBundleForSearchResponse ERROR:---" + e.Message);
            }

        }
        static bool fnTaskBundleForSearchResponse(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle TaskBundleForSearchResponse = new Bundle();
                TaskBundleForSearchResponse = populateTaskBundleForSearchResponse();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(TaskBundleForSearchResponse, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated TaskBundleForSearchResponse bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("TaskBundleForSearchResponse.json", TaskBundleForSearchResponse);
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

        static Bundle populateTaskBundleForSearchResponse()
        {
            // Set metadata about the resource            
            Bundle TaskBundleForSearchResponse = new Bundle()
            {
                // Set logical id of this artifact
                Id = "TaskBundleForSearchResponse-01",
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/TaskBundle",
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
            TaskBundleForSearchResponse.Identifier = identifier;

            // Set Bundle Type 
            TaskBundleForSearchResponse.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2023-12-13T15:32:26.605+05:30";
            TaskBundleForSearchResponse.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:36dc3058-fdf9-4765-9552-06f0c2a0c635";                                //Task/Task-06
            bundleEntry1.Resource = ResourcePopulator.populateTaskSearchResponse();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:03acb7b0-3833-45a0-9885-ad35940a3458";
            bundleEntry2.Resource = ResourcePopulator.populateClaimResponsesettlement();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:4776dbdf-d596-4cd1-9966-9d44ae9dec0b";
            bundleEntry3.Resource = ResourcePopulator.populateClaimsettlement();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry3);           


            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";
            bundleEntry4.Resource = ResourcePopulator.populatePatientResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry5.Resource = ResourcePopulator.populateOrganizationResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry5);          

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:b8970fa7-ae4a-47b4-9405-e382b3f7c055";
            bundleEntry6.Resource = ResourcePopulator.populateMedicationRequestResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";
            bundleEntry7.Resource = ResourcePopulator.populatePractitionerResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";
            bundleEntry8.Resource = ResourcePopulator.populateCoverageResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry8);

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4";
            bundleEntry9.Resource = ResourcePopulator.populateConditionResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry9);

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:7aace234-5172-4126-a907-ace8745bd1a5";
            bundleEntry10.Resource = ResourcePopulator.populateClaimenhancementResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry10);

            var bundleEntry11 = new Bundle.EntryComponent();
            bundleEntry11.FullUrl = "urn:uuid:760ec93e-7ec8-4e82-b8a3-fe6512fccd8b";
            bundleEntry11.Resource = ResourcePopulator.populateClaimpreauthResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry11);

            var bundleEntry12 = new Bundle.EntryComponent();
            bundleEntry12.FullUrl = "urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e";
            bundleEntry12.Resource = ResourcePopulator.populateDocumentReferenceResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry12);

            var bundleEntry13 = new Bundle.EntryComponent();
            bundleEntry13.FullUrl = "urn:uuid:514bcad3-7bf0-43a0-b566-e8ecd815dc91";
            bundleEntry13.Resource = ResourcePopulator.populateSecondDocumentReferenceResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry13);

            var bundleEntry14 = new Bundle.EntryComponent();
            bundleEntry14.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";
            bundleEntry14.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            TaskBundleForSearchResponse.Entry.Add(bundleEntry14);

            return TaskBundleForSearchResponse;
        }
    }
}
