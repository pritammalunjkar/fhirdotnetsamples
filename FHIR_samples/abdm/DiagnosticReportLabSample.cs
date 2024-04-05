using System;
using Hl7.Fhir.Model;
using System.Collections.Generic;


namespace FHIR_Profile_Validation
{
    //The DiagnosticReportLabSample class populates, validates, parse and serializes Clinical Artifact - DiagnosticReport Lab
    class DiagnosticReportLabSample
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside DiagnosticReportLabSample");
                fnDiagnosticReportLabSample(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("DiagnosticReportLabSample ERROR:---" + e.Message);
            }

        }
        static bool fnDiagnosticReportLabSample(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle diagnosticReportLabBundle = new Bundle();
                diagnosticReportLabBundle = populateDiagnosticReportLabBundle();

                string strErr_OUT = "";
                bool isValid =ResourcePopulator.ValidateProfile(diagnosticReportLabBundle, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated DiagnosticReportLab bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("diagnosticReportLabBundle.json", diagnosticReportLabBundle);
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
        static Bundle populateDiagnosticReportLabBundle()
        {
            // Set metadata about the resource            
            Bundle diagnosticReportBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "DiagnosticReport-Lab-02",
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

            //// Set version-independent identifier for the Bundle
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
            bundleEntry1.FullUrl = "urn:uuid:8a29f8cc-c494-4e2d-ad2a-7ca80ced4741";
            bundleEntry1.Resource = ResourcePopulator.populateDiagnosticReportRecordLabCompositionResource();
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
            bundleEntry4.FullUrl = "urn:uuid:2efefe2d-1998-403e-a8dd-36b93e31d2c8";  
            bundleEntry4.Resource = ResourcePopulator.populateDiagonosticReportLabResource();
            diagnosticReportBundle.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24";
            bundleEntry5.Resource = ResourcePopulator.populateOrganizationResource();
            diagnosticReportBundle.Entry.Add(bundleEntry5);


            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            bundleEntry6.Resource = ResourcePopulator.populateDocumentReferenceResource();
            diagnosticReportBundle.Entry.Add(bundleEntry6);
            
            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:81f65384-1005-4605-a276-b274ae006d3b";                     //Observation/Observation-cholesterol
            bundleEntry7.Resource = ResourcePopulator.populateCholesterolObservationResource();
            diagnosticReportBundle.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:aceb6f8a-44de-40f2-9928-bc940b45316e";                     //Observation/Observation-triglyceride
            bundleEntry8.Resource = ResourcePopulator.populateTriglycerideObservationResource();
            diagnosticReportBundle.Entry.Add(bundleEntry8);
 
            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:e64c7482-bde6-4b1f-95bc-2f23bf2ee333";                           //Observation/Observation-cholesterol-in-hdl
            bundleEntry9.Resource = ResourcePopulator.populateCholesterolInHDLObservationResource();
            diagnosticReportBundle.Entry.Add(bundleEntry9);


            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:6fbe092b-d72f-4d71-9ca0-90a3b247fa4c";                               //Specimen/Specimen-01
            bundleEntry10.Resource = ResourcePopulator.populateSpecimenResource();
            diagnosticReportBundle.Entry.Add(bundleEntry10);


            var bundleEntry11 = new Bundle.EntryComponent();
            bundleEntry11.FullUrl = "urn:uuid:aa8e4e90-c340-4140-9e12-c0acacc427f6";                               //Specimen/Specimen-01
            bundleEntry11.Resource = ResourcePopulator.populateServiceRequestResourceForLab();
            diagnosticReportBundle.Entry.Add(bundleEntry11);


            var bundleEntry12 = new Bundle.EntryComponent();
            bundleEntry12.FullUrl = "urn:uuid:aa0f5344-33ca-44a0-b8cf-9aa5b8a227ae";
            bundleEntry12.Resource = ResourcePopulator.populateSecondPractitionerResource();
            diagnosticReportBundle.Entry.Add(bundleEntry12);
            diagnosticReportBundle.Signature = ResourcePopulator.populateSignature();

            return diagnosticReportBundle;
        }
    }
}
