using FHIR_NHCX;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHCX_Sample_code
{
    class TaskBundleForCommunicationRequest
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside TaskBundleForCommunicationRequest");
                fnTaskBundleForCommunicationRequest(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("TaskBundleForCommunicationRequest ERROR:---" + e.Message);
            }

        }
        static bool fnTaskBundleForCommunicationRequest(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle TaskBundleForCommunicationRequest = new Bundle();
                TaskBundleForCommunicationRequest = populateTaskBundleForCommunicationRequest();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(TaskBundleForCommunicationRequest, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated TaskBundleForCommunicationRequest bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("TaskBundleForCommunicationRequest.json", TaskBundleForCommunicationRequest);
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
                 
        static Bundle populateTaskBundleForCommunicationRequest()
        {
            // Set metadata about the resource            
            Bundle TaskBundleForCommunicationRequest = new Bundle()
            {
                // Set logical id of this artifact
                Id ="TaskBundleForCommunicationRequest-01",
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
            TaskBundleForCommunicationRequest.Identifier = identifier;

            // Set Bundle Type 
            TaskBundleForCommunicationRequest.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2023-12-13T15:32:26.605+05:30";
            TaskBundleForCommunicationRequest.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:a8f27682-676d-4c2b-8af6-0540721311a0";               //Task/Task-01
            bundleEntry1.Resource = ResourcePopulator.populateTask();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:7061d007-2ac3-4cf2-b117-a2043f985c45";             //CommunicationRequest/CommunicationRequest-01
            bundleEntry2.Resource = ResourcePopulator.populateCommunicationRequest();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl ="urn:uuid:760ec93e-7ec8-4e82-b8a3-fe6512fccd8b";
            bundleEntry3.Resource = ResourcePopulator.populateClaimpreauthResource();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry3);
            
            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";
            bundleEntry4.Resource = ResourcePopulator.populatePatientResource();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry4);                    

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry5.Resource = ResourcePopulator.populateOrganizationResource();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry5);
            
            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";
            bundleEntry6.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9";
            bundleEntry7.Resource = ResourcePopulator.populateSecondMedicationRequestResource();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";
            bundleEntry8.Resource = ResourcePopulator.populatePractitionerResource();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry8);

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";
            bundleEntry9.Resource = ResourcePopulator.populateCoverageResource();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry9);

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4";
            bundleEntry10.Resource = ResourcePopulator.populateConditionResource();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry10);

            var bundleEntry11 = new Bundle.EntryComponent();
            bundleEntry11.FullUrl = "urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e";
            bundleEntry11.Resource = ResourcePopulator.populateDocumentReferenceResource();
            TaskBundleForCommunicationRequest.Entry.Add(bundleEntry11);

            return TaskBundleForCommunicationRequest;
        }
    }
}
