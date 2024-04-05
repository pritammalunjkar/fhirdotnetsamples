using FHIR_NHCX;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHCX_Sample_code
{
    class TaskBundleForCommunicationResponse
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside TaskBundleForCommunicationResponse");
                fnTaskBundleForCommunicationResponse(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("TaskBundleForCommunicationResponse ERROR:---" + e.Message);
            }

        }
        static bool fnTaskBundleForCommunicationResponse(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle TaskBundleForCommunicationResponse = new Bundle();
                TaskBundleForCommunicationResponse = populateTaskBundleForCommunicationResponse();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(TaskBundleForCommunicationResponse, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated TaskBundleForCommunicationResponse bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("TaskBundleForCommunicationResponse.json", TaskBundleForCommunicationResponse);
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

        static Bundle populateTaskBundleForCommunicationResponse()
        {
            // Set metadata about the resource            
            Bundle TaskBundleForCommunicationResponse = new Bundle()
            {
                // Set logical id of this artifact
                Id = "TaskBundleForCommunicationResponse-01",
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
            TaskBundleForCommunicationResponse.Identifier = identifier;

            // Set Bundle Type 
            TaskBundleForCommunicationResponse.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2023-12-13T15:32:26.605+05:30";
            TaskBundleForCommunicationResponse.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:a8f27682-676d-4c2b-8af6-0540721311a0";
            bundleEntry1.Resource = ResourcePopulator.populateTask();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:7c129ea5-e402-4162-9045-0ae659898dce";                // Communication/Communication-01
            bundleEntry2.Resource = ResourcePopulator.populateCommunication();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();

            bundleEntry3.FullUrl = "urn:uuid:7061d007-2ac3-4cf2-b117-a2043f985c45";
            bundleEntry3.Resource = ResourcePopulator.populateCommunicationRequest();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:760ec93e-7ec8-4e82-b8a3-fe6512fccd8b";
            bundleEntry4.Resource = ResourcePopulator.populateClaimpreauthResource();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";
            bundleEntry5.Resource = ResourcePopulator.populatePatientResource();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry5);

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry6.Resource = ResourcePopulator.populateOrganizationResource();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";
            bundleEntry7.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9";
            bundleEntry8.Resource = ResourcePopulator.populateSecondMedicationRequestResource();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry8);

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";
            bundleEntry9.Resource = ResourcePopulator.populatePractitionerResource();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry9);

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";
            bundleEntry10.Resource = ResourcePopulator.populateCoverageResource();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry10);

            var bundleEntry11 = new Bundle.EntryComponent();
            bundleEntry11.FullUrl = "urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4";
            bundleEntry11.Resource = ResourcePopulator.populateConditionResource();
            TaskBundleForCommunicationResponse.Entry.Add(bundleEntry11);

            return TaskBundleForCommunicationResponse;
        }
    }
}
