import { PDFDocument } from 'pdf-lib';
import fs from 'fs';

async function testPDF() {
    try {
        console.log('Iniciando prueba de PDF...');
        
        const templatePath = './wwwroot/uploads/formularios/Anexo #1 Interactivo.pdf';
        console.log('Leyendo template desde:', templatePath);
        
        if (!fs.existsSync(templatePath)) {
            throw new Error(`Template no encontrado: ${templatePath}`);
        }
        
        const existingPdfBytes = fs.readFileSync(templatePath);
        console.log('Template leído exitosamente, tamaño:', existingPdfBytes.length, 'bytes');
        
        const pdfDoc = await PDFDocument.load(existingPdfBytes);
        console.log('PDF cargado exitosamente');
        
        const form = pdfDoc.getForm();
        console.log('Formulario obtenido');
        
        // Intentar obtener los campos
        const fields = form.getFields();
        console.log('Campos encontrados:', fields.length);
        
        // Mostrar los nombres de los campos
        fields.forEach((field, index) => {
            console.log(`Campo ${index}: ${field.getName()} (${field.constructor.name})`);
        });
        
        // Intentar llenar algunos campos de prueba
        try {
            const testField = form.getTextField('NOMBREPROYECTO');
            testField.setText('Proyecto de Prueba');
            console.log('Campo NOMBREPROYECTO llenado exitosamente');
        } catch (fieldError) {
            console.log('Error llenando campo NOMBREPROYECTO:', fieldError.message);
        }
        
        const pdfBytes = await pdfDoc.save();
        fs.writeFileSync('./test_output.pdf', pdfBytes);
        console.log('PDF de prueba guardado exitosamente como test_output.pdf');
        
    } catch (error) {
        console.error('Error en prueba de PDF:', error);
        process.exit(1);
    }
}

testPDF();
