using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Slides;

namespace Medilearn.Services.Services
{
    public class PowerPointConversionService
    {
        public string ConvertPptToPdf(string pptFilePath, string pdfOutputPath)
        {
            using (Presentation presentation = new Presentation(pptFilePath))
            {
                presentation.Save(pdfOutputPath, Aspose.Slides.Export.SaveFormat.Pdf);
            }
            return pdfOutputPath;
        }
    }
}
