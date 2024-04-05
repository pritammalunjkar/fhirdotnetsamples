using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.IO;
using Hl7.Fhir.Validation;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;
using Newtonsoft.Json;
using System.Xml;
using FHIR_NHCX;

namespace NHCX_Sample_code
{
    class ClaimBundle_enhancement
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside ClaimBundleResource_enhancement");
                fnClaimBundleResource_enhancement(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("ClaimBundleResource_enhancement ERROR:---" + e.Message);
            }

        }
        static bool fnClaimBundleResource_enhancement(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle ClaimBundleResource_enhancement = new Bundle();
                ClaimBundleResource_enhancement = populateClaimBundleResource_enhancement();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(ClaimBundleResource_enhancement, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated ClaimBundleResource_enhancement bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("ClaimBundleResource_enhancement.json", ClaimBundleResource_enhancement);
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
        static Bundle populateClaimBundleResource_enhancement()
        {
            // Set metadata about the resource            
            Bundle ClaimBundleResource_enhancement = new Bundle()
            {
                // Set logical id of this artifact
                Id = "ClaimBundle-enhancement-01",
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
            ClaimBundleResource_enhancement.Identifier = identifier;

            // Set Bundle Type 
            ClaimBundleResource_enhancement.Type = Bundle.BundleType.Collection;

            ////// Set Timestamp  
            var dtStr ="2023-12-13T15:32:26.605+05:30";
            ClaimBundleResource_enhancement.TimestampElement = new Instant(DateTime.Parse(dtStr));
             
            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:7aace234-5172-4126-a907-ace8745bd1a5";    // Claim/Claim-enhancement-01
            bundleEntry1.Resource = ResourcePopulator.populateClaimenhancementResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:1efe03bf-9506-40ba-bc9a-80b0d5045afe";   // Patient/Patient-01
            bundleEntry2.Resource = ResourcePopulator.populatePatientResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:3a947161-4033-45d1-8b9c-7e9115c6000f";   //Organization/Organization-01
            bundleEntry3.Resource = ResourcePopulator.populateOrganizationResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry3);


            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:4a88bdc0-d320-4138-8014-d41913d9745a";   //Organization/Organization-03
            bundleEntry4.Resource = ResourcePopulator.populateHospitalOrganizationResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:3bc96820-c7c9-4f59-900d-6b0ed1fa558e";   //Practitioner/Practitioner-01
            bundleEntry5.Resource = ResourcePopulator.populatePractitionerResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry5);

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:0316e4d7-03e6-48b8-bcfd-8a3254f3f7b5";   //Coverage/Coverage-01";
            bundleEntry6.Resource = ResourcePopulator.populateCoverageResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:760ec93e-7ec8-4e82-b8a3-fe6512fccd8b";  //Claim/Claim-preauth-01;
            bundleEntry7.Resource = ResourcePopulator.populateClaimpreauthResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry7);
            
            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:acefdfbd-e612-483e-90fc-a5c44d09a4b9";   // MedicationRequest/MedicationRequest-01";
            bundleEntry8.Resource = ResourcePopulator.populateSecondMedicationRequestResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry8);               

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:bdaebfe7-8296-4241-9629-b16c364a10b4";   //Condition/Condition-01";
            bundleEntry9.Resource = ResourcePopulator.populateConditionResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry9);

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:e53fa5db-f676-4b16-a273-f4088866314e";    // DocumentReference/DocumentReference-01;
            bundleEntry10.Resource = ResourcePopulator.populateDocumentReferenceResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry10);

            var bundleEntry11 = new Bundle.EntryComponent();
            bundleEntry11.FullUrl ="urn:uuid:514bcad3-7bf0-43a0-b566-e8ecd815dc91";      //DocumentReference/DocumentReference-02;
            bundleEntry11.Resource = ResourcePopulator.populateSecondDocumentReferenceResource();
            ClaimBundleResource_enhancement.Entry.Add(bundleEntry11);

            return ClaimBundleResource_enhancement;
        }

    }
}
