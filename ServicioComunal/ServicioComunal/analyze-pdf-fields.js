import fs from 'fs';
import { PDFDocument } from 'pdf-lib';

async function analyzePdfFields(pdfPath, pdfName) {
    try {
        console.log(`\n=== ANALIZANDO: ${pdfName} ===`);
        console.log(`Archivo: ${pdfPath}`);
        
        const existingPdfBytes = fs.readFileSync(pdfPath);
        const pdfDoc = await PDFDocument.load(existingPdfBytes);
        const form = pdfDoc.getForm();
        const fields = form.getFields();
        
        console.log(`Total de campos encontrados: ${fields.length}`);
        console.log('\nCampos del formulario:');
        
        fields.forEach((field, index) => {
            const fieldName = field.getName();
            const fieldType = field.constructor.name;
            console.log(`${index + 1}. "${fieldName}" (${fieldType})`);
        });
        
        console.log(`\n=== FIN ANÁLISIS: ${pdfName} ===\n`);
        
    } catch (error) {
        console.error(`Error analizando ${pdfName}:`, error.message);
    }
}

async function main() {
    const basePath = 'c:/Users/luisd/mi_proyecto/ServicioComunal/ServicioComunal/wwwroot/uploads/formularios/';
    
    await analyzePdfFields(basePath + 'Informe final tutor Interactiva.pdf', 'INFORME FINAL TUTOR');
    await analyzePdfFields(basePath + 'Carta para ingresar a la institucion interactiva.pdf', 'CARTA INSTITUCIÓN');
    await analyzePdfFields(basePath + 'Carta de consentimiento encargado legal Interactiva.pdf', 'CARTA CONSENTIMIENTO');
}

main().catch(console.error);
