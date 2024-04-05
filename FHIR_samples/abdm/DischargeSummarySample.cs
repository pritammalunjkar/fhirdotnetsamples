using System;
using Hl7.Fhir.Model;
using System.Collections.Generic;

namespace FHIR_Profile_Validation
{
    //The DischargeSummarySample class populates, validates, parse and serializes Clinical Artifact - DischargeSummary
    class DischargeSummarySample
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside DischargeSummarySample");
                fnDischargeSummarySample(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("DischargeSummarySample ERROR:---" + e.Message);
            }

        }
        static bool fnDischargeSummarySample(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle dischargeSummaryBundle = new Bundle();
                dischargeSummaryBundle = populateDischargeSummaryBundle();

                string strErr_OUT = "";
                bool isValid =ResourcePopulator.ValidateProfile(dischargeSummaryBundle, ref strErr_OUT);
                //   isValid = true;
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated DischargeSummary bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("DischargeSummaryBundle.json", dischargeSummaryBundle);
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
        static Bundle populateDischargeSummaryBundle()
        {
            // Set metadata about the resource
            Bundle dischargeSummaryBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "discharge-bundle-01-my",
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
            identifier.ElementId = "BundelID";
            identifier.Value = "eba2ef3a-320f-4f16-8789-ed64965943a3";
            identifier.System = "http://hip.in";
            dischargeSummaryBundle.Identifier = identifier;

            // Set Bundle Type 
            dischargeSummaryBundle.Type = Bundle.BundleType.Document;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            dischargeSummaryBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:d687fc03-703f-4a32-9d90-d0691db92898";
            bundleEntry1.Resource = ResourcePopulator.populateDischargeSummaryCompositionResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147";
            bundleEntry2.Resource = ResourcePopulator.populatePractitionerResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry2);

            var bundleEntry3 = new Bundle.EntryComponent();          
            bundleEntry3.FullUrl = "urn:uuid:68ff0f24-3698-4877-b0ab-26e046fbec24";
            bundleEntry3.Resource = ResourcePopulator.populateOrganizationResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry3);

            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134";
            bundleEntry4.Resource = ResourcePopulator.populatePatientResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry4);
            
            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:c12a5b45-f88e-4811-be37-9d99658e5bc2";                                      //Encounter/Encounter-02
            bundleEntry5.Resource = ResourcePopulator.populateSecondEncounterResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry5);         

            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:aacc6b0f-771a-4ebf-a19c-5317389a92fd";                                        //Condition/Condition-01
            bundleEntry6.Resource = ResourcePopulator.populateFourthConditionResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry6); 
            
            var bundleEntry8 = new Bundle.EntryComponent();
            bundleEntry8.FullUrl = "urn:uuid:2efefe2d-1998-403e-a8dd-36b93e31d2c8";
            bundleEntry8.Resource = ResourcePopulator.populateDiagonosticReportLabResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry8);

            var bundleEntry9 = new Bundle.EntryComponent();
            bundleEntry9.FullUrl = "urn:uuid:81f65384-1005-4605-a276-b274ae006d3b";
            bundleEntry9.Resource = ResourcePopulator.populateCholesterolObservationResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry9);

            var bundleEntry10 = new Bundle.EntryComponent();
            bundleEntry10.FullUrl = "urn:uuid:aceb6f8a-44de-40f2-9928-bc940b45316e";
            bundleEntry10.Resource = ResourcePopulator.populateTriglycerideObservationResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry10);

            var bundleEntry11 = new Bundle.EntryComponent();
            bundleEntry11.FullUrl = "urn:uuid:e64c7482-bde6-4b1f-95bc-2f23bf2ee333";
            bundleEntry11.Resource = ResourcePopulator.populateCholesterolInHDLObservationResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry11);

            var bundleEntry12 = new Bundle.EntryComponent();
            bundleEntry12.FullUrl = "urn:uuid:8de92e2a-e761-41d5-a44b-152feac98bec";                      //Procedure/Procedure-01
            bundleEntry12.Resource = ResourcePopulator.populateProcedureResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry12);

            var bundleEntry13 = new Bundle.EntryComponent();
            bundleEntry13.FullUrl = "urn:uuid:57c44e5b-ad77-43e9-9654-27b3b9a4342e";                                         //Procedure/Procedure-02
            bundleEntry13.Resource = ResourcePopulator.populateSecondProcedureResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry13);

            var bundleEntry14 = new Bundle.EntryComponent();
            bundleEntry14.FullUrl = "urn:uuid:ea0a3b8c-ec4a-4ae2-ae86-23191a6c201c";                                         //CarePlan/CarePlan-01
            bundleEntry14.Resource = ResourcePopulator.populateSecondCarePlanResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry14);

            var bundleEntry15 = new Bundle.EntryComponent();             
            bundleEntry15.FullUrl = "urn:uuid:ff92b549-f754-4e3c-aef2-b403c99f6340";
            bundleEntry15.Resource = ResourcePopulator.populateDocumentReferenceResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry15);

            var bundleEntry16 = new Bundle.EntryComponent();
            bundleEntry16.FullUrl = "urn:uuid:1aeb371d-bb52-469a-9169-55a55aabb4bb";                                                   //Appointment/Appointment-01
            bundleEntry16.Resource = ResourcePopulator.populateAppointmentResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry16);

            var bundleEntry17 = new Bundle.EntryComponent();
            bundleEntry17.FullUrl = "urn:uuid:6fbe092b-d72f-4d71-9ca0-90a3b247fa4c";
            bundleEntry17.Resource = ResourcePopulator.populateSpecimenResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry17);

            var bundleEntry18 = new Bundle.EntryComponent();
            bundleEntry18.FullUrl = "urn:uuid:ee2af06a-903d-4387-8ed2-49f89d7da68d";                                 //MedicationRequest/MedicationRequest-01
            bundleEntry18.Resource = ResourcePopulator.populateMedicationRequestResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry18);


            var bundleEntry19 = new Bundle.EntryComponent();
            bundleEntry19.FullUrl = "urn:uuid:b939ec3c-b18a-4af0-a1a2-200db11ee8cb";                                 //MedicationRequest/MedicationRequest-01
            bundleEntry19.Resource = ResourcePopulator.populateServiceRequestResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry19);

            var bundleEntry20 = new Bundle.EntryComponent();
            bundleEntry20.FullUrl = "urn:uuid:b9768f37-82cb-471e-934f-71b9ce233656";
            bundleEntry20.Resource = ResourcePopulator.populateSecondOrganizationResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry20);

            var bundleEntry21 = new Bundle.EntryComponent();
            bundleEntry21.FullUrl = "urn:uuid:aa8e4e90-c340-4140-9e12-c0acacc427f6";                                 //MedicationRequest/MedicationRequest-01
            bundleEntry21.Resource = ResourcePopulator.populateServiceRequestResourceForLab();
            dischargeSummaryBundle.Entry.Add(bundleEntry21);

            var bundleEntry22 = new Bundle.EntryComponent();
            bundleEntry22.FullUrl = "urn:uuid:aa0f5344-33ca-44a0-b8cf-9aa5b8a227ae";                                 //MedicationRequest/MedicationRequest-01
            bundleEntry22.Resource = ResourcePopulator.populateSecondPractitionerResource();
            dischargeSummaryBundle.Entry.Add(bundleEntry22);
            



            dischargeSummaryBundle.Signature = ResourcePopulator.populateSignature();

            return dischargeSummaryBundle;
        }
    }
}
