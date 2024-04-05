using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.IO;
using Hl7.Fhir.Validation;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;
using System.Text;

namespace FHIR_Profile_Validation
{
   // The PrescriptionSample class populates, validates, parse and serializes Clinical Artifact - Prescription
    class PrescriptionSample
    {
        public static void Main()
        {
            try
            {
                string strErrOut = "";
                Console.WriteLine("Inside PrescriptionSample");
                fnPrescriptionSample(ref strErrOut);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("PrescriptionSample ERROR:---" +e.Message);
            }
            
        }

        static bool fnPrescriptionSample(ref string strError_OUT)
        {
            bool blnReturn = true;
            try
            {
                Bundle PrescriptionBundle = new Bundle();
                PrescriptionBundle = populatePrescriptionBundle();

                string strErr_OUT = "";
                bool isValid = ResourcePopulator.ValidateProfile(PrescriptionBundle, ref strErr_OUT);
                if (isValid != true)
                {
                    Console.WriteLine(strErr_OUT);
                }
                else
                {
                    Console.WriteLine("Validated populated Prescription bundle successfully");
                    bool isProfileCreated = ResourcePopulator.seralize_WriteFile("PrescriptionBundle.json", PrescriptionBundle);
                    if (isProfileCreated == false)
                    {
                        Console.WriteLine("Error in Profile File creation");
                        blnReturn = false;
                    }
                    else
                    {
                        Console.WriteLine("Prescription bundle file created Successfully");
                        blnReturn = true;
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
        static Bundle populatePrescriptionBundle()
        {
            // Set metadata about the resource
            Bundle prescriptionBundle = new Bundle()
            {
                // Set logical id of this artifact
                Id = "Prescription-01",
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
            identifier.Value = "bc3c6c57-2053-4d0e-ac40-139ccccff645";
            identifier.System = "http://hip.in";
            prescriptionBundle.Identifier = identifier;

            // Set Bundle Type 
            prescriptionBundle.Type = Bundle.BundleType.Document;

            ////// Set Timestamp  
            var dtStr = "2020-07-09T15:32:26.605+05:30";
            prescriptionBundle.TimestampElement = new Instant(DateTime.Parse(dtStr));

            var bundleEntry1 = new Bundle.EntryComponent();
            bundleEntry1.FullUrl = "urn:uuid:d687fc03-703f-4a32-9d90-d0691db92898";                    //Composition/Composition-01
            bundleEntry1.Resource = ResourcePopulator.populatePrescriptionCompositionResource();
            prescriptionBundle.Entry.Add(bundleEntry1);

            var bundleEntry2 = new Bundle.EntryComponent();
            bundleEntry2.FullUrl = "urn:uuid:23986a85-fb64-48c2-ab85-3462586cc134";
            bundleEntry2.Resource = ResourcePopulator.populatePatientResource();
            prescriptionBundle.Entry.Add(bundleEntry2);


            var bundleEntry3 = new Bundle.EntryComponent();
            bundleEntry3.FullUrl = "urn:uuid:86c1ae40-b60e-49b5-b2f4-a217bcd19147";
            bundleEntry3.Resource = ResourcePopulator.populatePractitionerResource();
            prescriptionBundle.Entry.Add(bundleEntry3);


            var bundleEntry4 = new Bundle.EntryComponent();
            bundleEntry4.FullUrl = "urn:uuid:ee2af06a-903d-4387-8ed2-49f89d7da68d";
            bundleEntry4.Resource = ResourcePopulator.populateMedicationRequestResource();
            prescriptionBundle.Entry.Add(bundleEntry4);

            var bundleEntry5 = new Bundle.EntryComponent();
            bundleEntry5.FullUrl = "urn:uuid:40d49bc0-9ac0-49c7-a3cb-de3da68b431f";                               //MedicationRequest/MedicationRequest-02
            bundleEntry5.Resource = ResourcePopulator.populateSecondMedicationRequestResource();
            prescriptionBundle.Entry.Add(bundleEntry5);


            var bundleEntry6 = new Bundle.EntryComponent();
            bundleEntry6.FullUrl = "urn:uuid:aacc6b0f-771a-4ebf-a19c-5317389a92fd";
            bundleEntry6.Resource = ResourcePopulator.populateFourthConditionResource();
            prescriptionBundle.Entry.Add(bundleEntry6);


            var bundleEntry7 = new Bundle.EntryComponent();
            bundleEntry7.FullUrl = "urn:uuid:859a3e51-5027-486a-bb41-c7773300fd40";                             //Binary/Binary-01
            bundleEntry7.Resource = ResourcePopulator.populateBinaryResource();
            prescriptionBundle.Entry.Add(bundleEntry7);
                      
            prescriptionBundle.Signature = ResourcePopulator.populateSignature();

            return prescriptionBundle;
        }

       
    }
}
