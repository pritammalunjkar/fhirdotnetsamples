using FHIR_NHCX;
using Hl7.Fhir.Model;

using System;
using System.Collections.Generic;

namespace NHCX_Sample_code
{
    class TaskBundleForPaymentNoticeRequest
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside TaskBundleForPaymentNoticeRequest");
                fnTaskBundleForPaymentNoticeRequest(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("TaskBundleForPaymentNoticeRequest ERROR:---" + e.Message);
            }

        }
        static bool fnTaskBundleForPaymentNoticeRequest(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle TaskBundleForPaymentNoticeRequest = new Bundle();
                TaskBundleForPaymentNoticeRequest = populateTaskBundleForPaymentNoticeRequest();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(TaskBundleForPaymentNoticeRequest, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated TaskBundleForPaymentNoticeRequest bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("TaskBundleForPaymentNoticeRequest.json", TaskBundleForPaymentNoticeRequest);
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

        static Bundle populateTaskBundleForPaymentNoticeRequest()
        {
            // Set metadata about the resource            
            Bundle TaskBundleForPaymentNoticeRequest = new Bundle()
            {
                // Set logical id of this artifact
                Id = "TaskBundleForPaymentNoticeRequest-01",
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
            TaskBundleForPaymentNoticeRequest.Identifier = identifier;

            // Set Bundle Type 
            TaskBundleForPaymentNoticeRequest.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2023-12-13T15:32:26.605+05:30";
            TaskBundleForPaymentNoticeRequest.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:75107ae5-8514-482a-bc38-88f6c82ccac1";                  //Task/Task-02
            bundleEntry1.Resource = ResourcePopulator.populateSecondTask();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();                     
            bundleEntry2.FullUrl = "urn:uuid:86e38f6d-4af6-4d24-aa7f-8bd774bf5080";                           //Task/PaymentNotice-01
            bundleEntry2.Resource = ResourcePopulator.populatePaymentNotice();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry2);


            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:4776dbdf-d596-4cd1-9966-9d44ae9dec0b";
            bundleEntry3.Resource = ResourcePopulator.populateClaimsettlement();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";
            bundleEntry4.Resource = ResourcePopulator.populatePatientResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry5.Resource = ResourcePopulator.populateOrganizationResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry5);

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";
            bundleEntry6.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9";
            bundleEntry7.Resource = ResourcePopulator.populateSecondMedicationRequestResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";
            bundleEntry8.Resource = ResourcePopulator.populatePractitionerResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry8);

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";
            bundleEntry9.Resource = ResourcePopulator.populateCoverageResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry9);

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4";
            bundleEntry10.Resource = ResourcePopulator.populateConditionResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry10);
            
            var bundleEntry11 = new Bundle.EntryComponent();
            bundleEntry11.FullUrl = "urn:uuid:7aace234-5172-4126-a907-ace8745bd1a5";
            bundleEntry11.Resource = ResourcePopulator.populateClaimenhancementResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry11);

            var bundleEntry12 = new Bundle.EntryComponent();
            bundleEntry12.FullUrl = "urn:uuid:760ec93e-7ec8-4e82-b8a3-fe6512fccd8b";
            bundleEntry12.Resource = ResourcePopulator.populateClaimpreauthResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry12);

            var bundleEntry13 = new Bundle.EntryComponent();
            bundleEntry13.FullUrl = "urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e";
            bundleEntry13.Resource = ResourcePopulator.populateDocumentReferenceResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry13);

            var bundleEntry14 = new Bundle.EntryComponent();
            bundleEntry14.FullUrl = "urn:uuid:514bcad3-7bf0-43a0-b566-e8ecd815dc91";
            bundleEntry14.Resource = ResourcePopulator.populateSecondDocumentReferenceResource();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry14);
            
            var bundleEntry15 = new Bundle.EntryComponent();
            bundleEntry15.FullUrl = "urn:uuid:e3fb872f-596c-4d6c-8e99-b246f4f10690";                //PaymentReconciliation/PaymentReconciliation-01
            bundleEntry15.Resource = ResourcePopulator.populatePaymentReconciliation();
            TaskBundleForPaymentNoticeRequest.Entry.Add(bundleEntry15);

            return TaskBundleForPaymentNoticeRequest;
        }
    }
}
