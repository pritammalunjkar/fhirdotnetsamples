using System;
using Hl7.Fhir.Model;
using System.Collections.Generic;

namespace FHIR_Profile_Validation
{
    class HealthDocumentRecordSample
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside HealthDocumentRecordSample");
                fnHealthDocumentRecordSample(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("HealthDocumentRecordSample ERROR:---" + e.Message);
            }

        }
        static bool fnHealthDocumentRecordSample(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle HealthDocumentRecordBundle = new Bundle();
                HealthDocumentRecordBundle = populateHealthDocumentRecordSample();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(HealthDocumentRecordBundle, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated HealthDocumentRecord bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("HealthDocumentRecordBundle.json", HealthDocumentRecordBundle);
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
        static Bundle populateHealthDocumentRecordSample()
        {
            // Set metadata about the resource            
            Bundle HealthDocumentRecordBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "HealthDocumentRecord-01",
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
            HealthDocumentRecordBundle.Identifier = identifier;

            // Set Bundle Type 
            HealthDocumentRecordBundle.Type = Bundle.BundleType.Document;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            HealthDocumentRecordBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:ecae12dd-9966-41f0-b44b-8d3eabf14111";
            bundleEntry1.Resource = ResourcePopulator.populateHealthDocumentRecordCompositionResource();
            HealthDocumentRecordBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147";
            bundleEntry2.Resource = ResourcePopulator.populatePractitionerResource();
            HealthDocumentRecordBundle.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134";
            bundleEntry3.Resource = ResourcePopulator.populatePatientResource();
            HealthDocumentRecordBundle.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            bundleEntry4.Resource = ResourcePopulator.populateDocumentReferenceResource();
            HealthDocumentRecordBundle.Entry.Add(bundleEntry4);

            HealthDocumentRecordBundle.Signature = ResourcePopulator.populateSignature();

            return HealthDocumentRecordBundle;
        }
    }
}
