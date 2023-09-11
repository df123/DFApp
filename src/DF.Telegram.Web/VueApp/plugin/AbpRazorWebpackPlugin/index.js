const pluginName = 'AbpRazorWebpackPlugin';
const fs = require('fs');
const os = require('os');
const path = require('path');

class AbpRazorWebpackPlugin {
    constructor(options) {
        this.options = options;
    }

    readCshtmlFiles(dirPath, results) {
        const files = fs.readdirSync(dirPath);
        for (let i = 0; i < files.length; i++) {
            const filePath = path.join(dirPath, files[i]);
            const stats = fs.statSync(filePath);
            if (stats.isDirectory()) {
                this.readCshtmlFiles(filePath, results);
            } else if (path.extname(filePath) === '.cshtml') {
                results[filePath] = this.readFile(filePath);
            }
        }
    }
    
    readFile(filePath) {
        try {
            let data = fs.readFileSync(filePath);
            return data.toString();
        } catch (error) {
            console.error('Error:', error);
        }
    }
    
    writeFile(filePath, modifiedData) {
        if (modifiedData) {
    
            try {
                fs.writeFileSync(filePath, modifiedData, 'utf8');
                console.log('File modified successfully!');
            } catch (error) {
                console.error('Error writing file:', err);
            }
        } else {
            console.log('No modifications made.');
        }
    }
    
    modifyFile(fileData, targetChar, newContent) {
    
        try {
            const lines = fileData.split(os.EOL);
            let startLine = null;
            let endLine = null;
            for (let i = 0; i < lines.length; i++) {
                if (lines[i].includes(targetChar)) {
                    if (startLine == null) {
                        startLine = i;
                    }
                    else if (endLine == null) {
                        endLine = i;
                    }
                    else {
                        break;
                    }
                }
            }

            if (startLine == null || endLine == null) {
                return null;
            }

            lines.splice(startLine + 1, (endLine - startLine) - 1);
            lines.splice(startLine + 1, 0, newContent)
    
            return lines.join(os.EOL);
        } catch (error) {
            console.error('Error:', error);
            return null;
        }
    }

    apply(compiler) {
        compiler.hooks.done.tap(pluginName, (stats) => {
            let cshtmlContents = {};
            this.readCshtmlFiles(this.options.htmlpath,cshtmlContents);

            for (const [name, entry] of stats.compilation.entrypoints) {
                let targetChar = `@* ${name} *@`
                let scriptStrs = [];
                for (const file of entry.getFiles()) {
                    const asset = stats.compilation.getAsset(file);
                    if (
                        asset &&
                        asset.info &&
                        asset.source &&
                        (asset.info.size > 0 || asset.source.size() > 0)
                    ) {

                        let scriptStr = `<abp-script src="/${this.options.path}/${asset.name}"></abp-script>`
                        scriptStrs.push(scriptStr);
                    }
                }
                for(const key in cshtmlContents){
                    const modifiedData = this.modifyFile(cshtmlContents[key], targetChar, scriptStrs.join(os.EOL));
                    if(modifiedData){
                        this.writeFile(key, modifiedData);
                    }
                }
                
            }
        })
    }
}

module.exports = AbpRazorWebpackPlugin;

