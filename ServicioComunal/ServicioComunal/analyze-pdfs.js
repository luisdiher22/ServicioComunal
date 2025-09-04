import { PDFDocument } from 'pdf-lib';
import fs from 'fs';
import path from 'path';

async function analyzePDF(pdfPath) {
    try {
        console.log(`\n=== ANALIZANDO: ${path.basename(pdfPath)} ===`);
        
        const pdfBytes = fs.readFileSync(pdfPath);
        const pdfDoc = await PDFDocument.load(pdfBytes);
        const form = pdfDoc.getForm();
        const fields = form.getFields();
        
        if (fields.length === 0) {
            console.log('❌ Este PDF no tiene campos interactivos');
            return [];
        }
        
        console.log(`✅ Encontrados ${fields.length} campos interactivos:\n`);
        
        const fieldInfo = [];
        
        fields.forEach((field, index) => {
            const fieldName = field.getName();
            const fieldType = field.constructor.name;
            
            console.log(`${index + 1}. Nombre: "${fieldName}"`);
            console.log(`   Tipo: ${fieldType}`);
            
            let additionalInfo = '';
            
            // Información adicional según el tipo de campo
            try {
                if (fieldType === 'PDFTextField') {
                    const maxLength = field.getMaxLength();
                    const isMultiline = field.isMultiline();
                    additionalInfo = `Max length: ${maxLength}, Multiline: ${isMultiline}`;
                } else if (fieldType === 'PDFDropdown') {
                    const options = field.getOptions();
                    additionalInfo = `Opciones: [${options.join(', ')}]`;
                } else if (fieldType === 'PDFRadioGroup') {
                    const options = field.getOptions();
                    additionalInfo = `Opciones: [${options.join(', ')}]`;
                } else if (fieldType === 'PDFCheckBox') {
                    const isChecked = field.isChecked();
                    additionalInfo = `Estado: ${isChecked ? 'Marcado' : 'No marcado'}`;
                }
                
                if (additionalInfo) {
                    console.log(`   Info: ${additionalInfo}`);
                }
            } catch (error) {
                console.log(`   Info: No disponible (${error.message})`);
            }
            
            console.log('');
            
            fieldInfo.push({
                name: fieldName,
                type: fieldType,
                info: additionalInfo
            });
        });
        
        return fieldInfo;
        
    } catch (error) {
        console.error(`❌ Error analizando ${path.basename(pdfPath)}:`, error.message);
        return [];
    }
}

async function analyzeAllPDFs() {
    const pdfDir = './wwwroot/uploads/formularios';
    const anexoPDFs = [
        'Anexo #1 Interactivo.pdf',
        'Anexo #2 Interactivo.pdf', 
        'Anexo #3 Interactivo.pdf',
        'Anexo #5 Interactivo.pdf'
    ];
    
    const allFieldsInfo = {};
    
    for (const pdfName of anexoPDFs) {
        const pdfPath = path.join(pdfDir, pdfName);
        
        if (fs.existsSync(pdfPath)) {
            const fieldsInfo = await analyzePDF(pdfPath);
            allFieldsInfo[pdfName] = fieldsInfo;
        } else {
            console.log(`❌ No se encontró: ${pdfPath}`);
        }
    }
    
    // Resumen final
    console.log('\n' + '='.repeat(60));
    console.log('RESUMEN DE TODOS LOS CAMPOS ENCONTRADOS');
    console.log('='.repeat(60));
    
    Object.entries(allFieldsInfo).forEach(([pdfName, fields]) => {
        console.log(`\n📄 ${pdfName}: ${fields.length} campos`);
        fields.forEach(field => {
            console.log(`   • ${field.name} (${field.type})`);
        });
    });
    
    // Generar código de ejemplo para cada PDF
    console.log('\n' + '='.repeat(60));
    console.log('CÓDIGO DE EJEMPLO PARA RELLENAR CADA PDF');
    console.log('='.repeat(60));
    
    Object.entries(allFieldsInfo).forEach(([pdfName, fields]) => {
        if (fields.length > 0) {
            console.log(`\n// === ${pdfName} ===`);
            console.log(`async function rellenar_${pdfName.replace(/[^a-zA-Z0-9]/g, '_')}(formData) {`);
            console.log(`    const form = pdfDoc.getForm();`);
            
            fields.forEach(field => {
                const fieldVar = field.name.replace(/[^a-zA-Z0-9]/g, '_').toLowerCase();
                
                if (field.type === 'PDFTextField') {
                    console.log(`    form.getTextField('${field.name}').setText(formData.${fieldVar} || '');`);
                } else if (field.type === 'PDFCheckBox') {
                    console.log(`    if (formData.${fieldVar}) form.getCheckBox('${field.name}').check();`);
                } else if (field.type === 'PDFDropdown') {
                    console.log(`    form.getDropdown('${field.name}').select(formData.${fieldVar});`);
                } else if (field.type === 'PDFRadioGroup') {
                    console.log(`    form.getRadioGroup('${field.name}').select(formData.${fieldVar});`);
                }
            });
            
            console.log(`}`);
        }
    });
}

// Ejecutar análisis
analyzeAllPDFs().catch(console.error);
