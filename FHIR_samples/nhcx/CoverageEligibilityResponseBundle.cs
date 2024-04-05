using System;
using Hl7.Fhir.Model;
using System.Collections.Generic;
using FHIR_NHCX;

namespace NHCX_Sample_code
{
    class CoverageEligibilityResponseBundle
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside CoverageEligibilityResponseBundle");
                fnCoverageEligibilityResponseBundle(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("CoverageEligibilityResponseBundle ERROR:---" + e.Message);
            }

        }

        static bool fnCoverageEligibilityResponseBundle(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle CoverageEligibilityResponseBundle = new Bundle();
                CoverageEligibilityResponseBundle = populateCoverageEligibilityResponseBundle();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(CoverageEligibilityResponseBundle, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated CoverageEligibilityResponseBundle bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("CoverageEligibilityResponseBundle.json", CoverageEligibilityResponseBundle);
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

        static Bundle populateCoverageEligibilityResponseBundle()
        {
            // Set metadata about the resource            
            Bundle coverageEligibilityResponseBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "CoverageEligibilityResponseBundle-example-01",
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/CoverageEligibilityResponseBundle",
                    },
                    // Set Confidentiality as defined by affinity domain
                    Security = new List<Coding>()
                    {
                        new Coding("http://terminology.hl7.org/CodeSystem/v3-Confidentiality", "V", "very restricted"),
                    }
                },
            };

            // Set Bundle Type 
            coverageEligibilityResponseBundle.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            coverageEligibilityResponseBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            CoverageEligibilityResponse.EligibilityResponsePurpose CoverageEligibilityResponsePurpose =  CoverageEligibilityResponse.EligibilityResponsePurpose.AuthRequirements;
            CoverageEligibilityRequest.EligibilityRequestPurpose   CoverageEligibilityRequestPurpose =  CoverageEligibilityRequest.EligibilityRequestPurpose.AuthRequirements ;


            Console.WriteLine("Please Choose the purpose for the CoverageEligbilityResponse \nEnter 1 for auth-requirements\nEnter 2 for benefits\nEnter 3 for discovery\nEnter 4 for validation");
            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    CoverageEligibilityResponsePurpose = CoverageEligibilityResponse.EligibilityResponsePurpose.AuthRequirements;                     
                    CoverageEligibilityRequestPurpose = CoverageEligibilityRequest.EligibilityRequestPurpose.AuthRequirements;
                    break;

                case 2:
                    CoverageEligibilityResponsePurpose = CoverageEligibilityResponse.EligibilityResponsePurpose.Benefits;
                    CoverageEligibilityRequestPurpose = CoverageEligibilityRequest.EligibilityRequestPurpose.Benefits;
                    break;

                case 3:
                    CoverageEligibilityResponsePurpose = CoverageEligibilityResponse.EligibilityResponsePurpose.Discovery;
                    CoverageEligibilityRequestPurpose = CoverageEligibilityRequest.EligibilityRequestPurpose.Discovery;
                    break;

                case 4:
                    CoverageEligibilityResponsePurpose = CoverageEligibilityResponse.EligibilityResponsePurpose.Validation;
                    CoverageEligibilityRequestPurpose = CoverageEligibilityRequest.EligibilityRequestPurpose.Validation;
                    break;

                default:
                    Console.WriteLine("Wrong input");
                    break;
            }

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:9310eab0-febd-42c3-8590-2f03e6c7dfca";    // "CoverageEligibilityResponse/CoverageEligibilityResponse-01"; 
            bundleEntry1.Resource = ResourcePopulator.populateCoverageEligiblityResponse(CoverageEligibilityResponsePurpose);
            coverageEligibilityResponseBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";
            bundleEntry2.Resource = ResourcePopulator.populatePatientResource();
            coverageEligibilityResponseBundle.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";
            bundleEntry3.Resource = ResourcePopulator.populatePractitionerResource();
            coverageEligibilityResponseBundle.Entry.Add(bundleEntry3);
            
            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:78787db5-bcf2-43a2-a01a-e843fd08bfe7";
            bundleEntry4.Resource = ResourcePopulator.populateCoverageEligibilityRequest(CoverageEligibilityRequestPurpose);
            coverageEligibilityResponseBundle.Entry.Add(bundleEntry4);           

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";
            bundleEntry5.Resource = ResourcePopulator.populateOrganizationResource();
            coverageEligibilityResponseBundle.Entry.Add(bundleEntry5);

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:1cb884ad-0df4-4c35-ae40-4764895c84c6";
            bundleEntry6.Resource = ResourcePopulator.populateLocationResource();
            coverageEligibilityResponseBundle.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";
            bundleEntry7.Resource = ResourcePopulator.populateCoverageResource();
            coverageEligibilityResponseBundle.Entry.Add(bundleEntry7);


            return coverageEligibilityResponseBundle;
        }
    }
}
