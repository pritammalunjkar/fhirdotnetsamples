using System;
using Hl7.Fhir.Model;
using System.Collections.Generic;

namespace FHIR_Profile_Validation
{
    class ImmunizationRecordSample
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside ImmunizationRecordSample");
                fnImmunizationRecordSample(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("ImmunizationRecordSample ERROR:---" + e.Message);
            }

        }

        static bool fnImmunizationRecordSample(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle immunizationRecordBundle = new Bundle();
                immunizationRecordBundle = populateImmunizationRecordBundle();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(immunizationRecordBundle, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated ImmunizationRecord bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("immunizationRecordBundle.json", immunizationRecordBundle);
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
        static Bundle populateImmunizationRecordBundle()
        {
            // Set metadata about the resource            
            Bundle ImmunizationRecordBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "ImmunizationRecord-01",
                Meta = new Meta()
                {
                    VersionId = "1",
                    LastUpdatedElement = new Instant(new DateTimeOffset(2020, 07, 09, 15, 32, 26, new TimeSpan(1, 0, 0))),
                    Profile = new List<string>()
                    {
                      "https://nrces.in/ndhm/fhir/r4/StructureDefinition/DocumentBundle",
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
            identifier.Value = "305fecc2-4ba2-46cc-9ccd-efa755aff51d";
            identifier.System = "http://hip.in";
            ImmunizationRecordBundle.Identifier = identifier;

            // Set Bundle Type 
            ImmunizationRecordBundle.Type = Bundle.BundleType.Document;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            ImmunizationRecordBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:c26f5e55-3049-4fde-80a4-7e4476be16dd";
            bundleEntry1.Resource = ResourcePopulator.populateImmunizationRecordCompositionResource();
            ImmunizationRecordBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147";
            bundleEntry2.Resource = ResourcePopulator.populatePractitionerResource();
            ImmunizationRecordBundle.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24";
            bundleEntry3.Resource = ResourcePopulator.populateOrganizationResource();
            ImmunizationRecordBundle.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134";
            bundleEntry4.Resource = ResourcePopulator.populatePatientResource();
            ImmunizationRecordBundle.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:34403a5b-4ae3-4996-9e76-10e9bc16476e";                                  //Immunization/Immunization-01
            bundleEntry5.Resource = ResourcePopulator.populateImmunizationResource();
            ImmunizationRecordBundle.Entry.Add(bundleEntry5);

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:9fcd0d40-e075-431f-ade5-a2e7b01858cd";                                           //ImmunizationRecommendation/ImmunizationRecommendation-01
            bundleEntry6.Resource = ResourcePopulator.populateImmunizationRecommendation();
            ImmunizationRecordBundle.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            bundleEntry7.Resource = ResourcePopulator.populateDocumentReferenceResource();
            ImmunizationRecordBundle.Entry.Add(bundleEntry7);

            ImmunizationRecordBundle.Signature = ResourcePopulator.populateSignature();
            return ImmunizationRecordBundle;
        }
    }
}
