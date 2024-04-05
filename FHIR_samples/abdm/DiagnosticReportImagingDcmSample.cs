using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;

namespace FHIR_Profile_Validation
{

    //The DiagnosticReportImagingDcmSample class populates, validates, parse and serializes Clinical Artifact - DiagnosticReport Imaging DCM

    class DiagnosticReportImagingDcmSample
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside DiagnosticReportImagingDcmSample");
                fnDiagnosticReportImagingDcmSample(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("DiagnosticReportImagingDcmSample ERROR:---" + e.Message);
            }

        }
        static bool fnDiagnosticReportImagingDcmSample(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle diagnosticReportImagingDCMBundle = new Bundle();
                diagnosticReportImagingDCMBundle = populateDiagnosticReportImagingDCMBundle();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(diagnosticReportImagingDCMBundle, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated DiagnosticReportImagingDCM bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("diagnosticReportImagingDCM.json", diagnosticReportImagingDCMBundle);
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
        static Bundle populateDiagnosticReportImagingDCMBundle()
        {
            // Set metadata about the resource            
            Bundle diagnosticReportBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "DiagnosticReport-Imaging-DCM-01",
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
            identifier.Value = "242590bb-b122-45d0-8eb2-883392297ee1";
            identifier.System = "http://hip.in";
            diagnosticReportBundle.Identifier = identifier;

            // Set Bundle Type 
            diagnosticReportBundle.Type = Bundle.BundleType.Document;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            diagnosticReportBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:d687fc03-703f-4a32-9d90-d0691db92898";                                 //Composition/Composition-01
            bundleEntry1.Resource = ResourcePopulator.populateDiagnosticReportRecordDCMCompositionResource();
            diagnosticReportBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();            
            bundleEntry2.FullUrl = "urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134";                                  //Patient/Patient-01 
            bundleEntry2.Resource = ResourcePopulator.populatePatientResource();
            diagnosticReportBundle.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147";                        //Practitioner/Practitioner-01
            bundleEntry3.Resource = ResourcePopulator.populatePractitionerResource();
            diagnosticReportBundle.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24";                        //Organization/Organization-01
            bundleEntry4.Resource = ResourcePopulator.populateOrganizationResource();
            diagnosticReportBundle.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:2efefe2d-1998-403e-a8dd-36b93e31d2c8";                                //DiagnosticReport/DiagnosticReport-01
            bundleEntry5.Resource = ResourcePopulator.populateDiagnosticReportImagingDCMResource();
            diagnosticReportBundle.Entry.Add(bundleEntry5);

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:29530ff8-baa7-4669-9afb-0b37fb4c6982";                         //ImagingStudy/ImagingStudy-01
            bundleEntry6.Resource = ResourcePopulator.populateImagingStudyResource();
            diagnosticReportBundle.Entry.Add(bundleEntry6);

            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:35e0e4fa-1d49-4aa4-bd82-5ae9338e8703";                             //Media/Media-01
            bundleEntry7.Resource = ResourcePopulator.populateMediaResource();
            diagnosticReportBundle.Entry.Add(bundleEntry7);

            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:b939ec3c-b18a-4af0-a1a2-200db11ee8cb";                                           //ServiceRequest/ServiceRequest-01
            bundleEntry8.Resource = ResourcePopulator.populateServiceRequestResource();
            diagnosticReportBundle.Entry.Add(bundleEntry8);

                    

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";                                //DocumentReference/DocumentReference-01
            bundleEntry10.Resource = ResourcePopulator.populateDocumentReferenceResource();
            diagnosticReportBundle.Entry.Add(bundleEntry10);

            diagnosticReportBundle.Signature = ResourcePopulator.populateSignature();
            return diagnosticReportBundle;
        }
    }
}
