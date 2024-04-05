using System;
using Hl7.Fhir.Model;
using System.Collections.Generic;
using FHIR_NHCX;

namespace NHCX_Sample_code
{
    class CoverageEligibilityRequestBundle
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside CoverageEligibilityRequestBundle");
                fnCoverageEligibilityRequestBundle(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("CoverageEligibilityRequestBundle ERROR:---" + e.Message);
            }

        }

        static bool fnCoverageEligibilityRequestBundle(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle CoverageEligibilityRequestBundle = new Bundle();
                CoverageEligibilityRequestBundle = populateCoverageEligibilityRequestBundle();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(CoverageEligibilityRequestBundle, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated CoverageEligibilityRequestBundle bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("CoverageEligibilityRequestBundle.json", CoverageEligibilityRequestBundle);
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

        static Bundle populateCoverageEligibilityRequestBundle()
        {
            // Set metadata about the resource            
            Bundle CoverageEligibilityRequestBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "CoverageEligibilityRequestBundle-01",
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/CoverageEligibilityRequestBundle",
                    },
                    // Set Confidentiality as defined by affinity domain
                    Security = new List<Coding>()
                    {
                        new Coding("http://terminology.hl7.org/CodeSystem/v3-Confidentiality", "V", "very restricted"),
                    }
                },
            };

            // Set Bundle Type 
            CoverageEligibilityRequestBundle.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            CoverageEligibilityRequestBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            CoverageEligibilityRequest.EligibilityRequestPurpose CoverageEligibilityRequestPurpose = CoverageEligibilityRequest.EligibilityRequestPurpose.AuthRequirements; 

            Console.WriteLine("Please Choose the purpose for the CoverageEligbilityRequest \nEnter 1 for auth-requirements\nEnter 2 for benefits\nEnter 3 for discovery\nEnter 4 for validation");
            int choice = Convert.ToInt32(Console.ReadLine());
                        
            switch (choice)
            {
                case 1:
                    CoverageEligibilityRequestPurpose = CoverageEligibilityRequest.EligibilityRequestPurpose.AuthRequirements;
                    break;

                case 2:
                    CoverageEligibilityRequestPurpose = CoverageEligibilityRequest.EligibilityRequestPurpose.Benefits;
                    break;

                case 3:
                    CoverageEligibilityRequestPurpose = CoverageEligibilityRequest.EligibilityRequestPurpose.Discovery;
                    break;

                case 4:
                    CoverageEligibilityRequestPurpose = CoverageEligibilityRequest.EligibilityRequestPurpose.Validation;
                    break;

                default:
                    Console.WriteLine("Wrong input");
                    break;
            }

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:78787db5-bcf2-43a2-a01a-e843fd08bfe7";         // "CoverageEligbilityRequest/CoverageEligibilityRequest-01";
            bundleEntry1.Resource = ResourcePopulator.populateCoverageEligibilityRequest(CoverageEligibilityRequestPurpose);
            CoverageEligibilityRequestBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";
            bundleEntry2.Resource = ResourcePopulator.populatePatientResource();
            CoverageEligibilityRequestBundle.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";
            bundleEntry3.Resource = ResourcePopulator.populatePractitionerResource();
            CoverageEligibilityRequestBundle.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry4.Resource = ResourcePopulator.populateOrganizationResource();
            CoverageEligibilityRequestBundle.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:1cb884ad-0df4-4c35-ae40-4764895c84c6";       // "Location/Location-01";
            bundleEntry5.Resource = ResourcePopulator.populateLocationResource();
            CoverageEligibilityRequestBundle.Entry.Add(bundleEntry5);            
            
            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";
            bundleEntry6.Resource = ResourcePopulator.populateCoverageResource();
            CoverageEligibilityRequestBundle.Entry.Add(bundleEntry6);
                      

            return CoverageEligibilityRequestBundle;
        }
    }
}
