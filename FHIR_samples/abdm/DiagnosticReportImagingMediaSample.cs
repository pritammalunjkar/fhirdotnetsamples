using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;

namespace FHIR_Profile_Validation
{
    //The DiagnosticReportImagingMediaSample class populates, validates, parse and serializes Clinical Artifact - DiagnosticReport Imaging Media
    class DiagnosticReportImagingMediaSample
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside DiagnosticReportImagingMediaSample");
                fnDiagnosticReportImagingMediaSample(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("DiagnosticReportImagingMediaSample ERROR:---" + e.Message);
            }

        }
        static bool fnDiagnosticReportImagingMediaSample(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle diagnosticReportImagingMediaBundle = new Bundle();
                diagnosticReportImagingMediaBundle = populateDiagnosticReportImagingMediaBundle();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(diagnosticReportImagingMediaBundle, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated DiagnosticReportImagingMedia bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("diagnosticReportImagingMediaBundle.json", diagnosticReportImagingMediaBundle);
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
        static Bundle populateDiagnosticReportImagingMediaBundle()
        {
            // Set metadata about the resource            
            Bundle diagnosticReportBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "DiagnosticReport-Imaging-Media-02",
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
            identifier.Value = "9932f74d-2812-4be0-bd50-ed76eeab301d";
            identifier.System = "http://hip.in";
            diagnosticReportBundle.Identifier = identifier;

            // Set Bundle Type 
            diagnosticReportBundle.Type = Bundle.BundleType.Document;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            diagnosticReportBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:254211bb-0b56-4f7b-a55e-100253c68c71";
            bundleEntry1.Resource = ResourcePopulator.populateDiagnosticReportRecordMediaCompositionResource();
            diagnosticReportBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134";
            bundleEntry2.Resource = ResourcePopulator.populatePatientResource();
            diagnosticReportBundle.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147";
            bundleEntry3.Resource = ResourcePopulator.populatePractitionerResource();
            diagnosticReportBundle.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24";
            bundleEntry4.Resource = ResourcePopulator.populateOrganizationResource();
            diagnosticReportBundle.Entry.Add(bundleEntry4);               

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:35e0e4fa-1d49-4aa4-bd82-5ae9338e8703";
            bundleEntry6.Resource = ResourcePopulator.populateMediaResource();
            diagnosticReportBundle.Entry.Add(bundleEntry6);


            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:3c598ce5-d1db-4d4d-b5e2-69f142396d55";
            bundleEntry8.Resource = ResourcePopulator.populateDiagnosticReportImagingMediaResource();
            diagnosticReportBundle.Entry.Add(bundleEntry8);


            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            bundleEntry9.Resource = ResourcePopulator.populateDocumentReferenceResource();
            diagnosticReportBundle.Entry.Add(bundleEntry9);



            diagnosticReportBundle.Signature = ResourcePopulator.populateSignature();

            return diagnosticReportBundle;
        }
    }
}
