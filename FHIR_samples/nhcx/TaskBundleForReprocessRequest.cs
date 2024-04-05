using FHIR_NHCX;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;

namespace NHCX_Sample_code
{
    class TaskBundleForReprocessRequest
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside TaskBundleForReprocessRequest");
                fnTaskBundleForReprocessRequest(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("TaskBundleForReprocessRequest ERROR:---" + e.Message);
            }

        }
        static bool fnTaskBundleForReprocessRequest(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle TaskBundleForReprocessRequest = new Bundle();
                TaskBundleForReprocessRequest = populateTaskBundleForReprocessRequest();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(TaskBundleForReprocessRequest, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated TaskBundleForReprocessRequest bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("TaskBundleForReprocessRequest.json", TaskBundleForReprocessRequest);
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

        static Bundle populateTaskBundleForReprocessRequest()
        {
            // Set metadata about the resource            
            Bundle TaskBundleForReprocessRequest = new Bundle()
            {
                // Set logical id of this artifact
                Id = "TaskBundleForReprocessRequest-01",
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
            TaskBundleForReprocessRequest.Identifier = identifier;

            // Set Bundle Type 
            TaskBundleForReprocessRequest.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2023-12-13T15:32:26.605+05:30";
            TaskBundleForReprocessRequest.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:23385f90-aa41-4e82-b212-d7a6191737a4";                 //Task/Task-04
            bundleEntry1.Resource = ResourcePopulator.populateTaskReprocessRequest();
            TaskBundleForReprocessRequest.Entry.Add(bundleEntry1);
                        
            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry2.Resource = ResourcePopulator.populateOrganizationResource();
            TaskBundleForReprocessRequest.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";
            bundleEntry3.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            TaskBundleForReprocessRequest.Entry.Add(bundleEntry3);                        

            return TaskBundleForReprocessRequest;
        }
    }
}
