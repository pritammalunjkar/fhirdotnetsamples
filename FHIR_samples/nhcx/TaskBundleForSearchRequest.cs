using FHIR_NHCX;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;

namespace NHCX_Sample_code
{
    class TaskBundleForSearchRequest
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside TaskBundleForSearchRequest");
                fnTaskBundleForSearchRequest(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("TaskBundleForSearchRequest ERROR:---" + e.Message);
            }

        }
        static bool fnTaskBundleForSearchRequest(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle TaskBundleForSearchRequest = new Bundle();
                TaskBundleForSearchRequest = populateTaskBundleForSearchRequest();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(TaskBundleForSearchRequest, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated TaskBundleForSearchRequest bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("TaskBundleForSearchRequest.json", TaskBundleForSearchRequest);
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

        static Bundle populateTaskBundleForSearchRequest()
        {
            // Set metadata about the resource            
            Bundle TaskBundleForSearchRequest = new Bundle()
            {
                // Set logical id of this artifact
                Id = "TaskBundleForSearchRequest-01",
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
            TaskBundleForSearchRequest.Identifier = identifier;

            // Set Bundle Type 
            TaskBundleForSearchRequest.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2023-12-13T15:32:26.605+05:30";
            TaskBundleForSearchRequest.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:fd549b56-c569-4a16-ab17-b52744230d73";                        //Task/Task-05
            bundleEntry1.Resource = ResourcePopulator.populateTaskSearchRequest();
            TaskBundleForSearchRequest.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry2.Resource = ResourcePopulator.populateOrganizationResource();
            TaskBundleForSearchRequest.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";
            bundleEntry3.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            TaskBundleForSearchRequest.Entry.Add(bundleEntry3);

            return TaskBundleForSearchRequest;
        }
    }
}
