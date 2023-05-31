// format html`...` by vscode lit-html
function html(strings, ...values) {
    let output = '';
    let index;
    for (index = 0; index < values.length; index += 1) {
        output += strings[index] + values[index];
    }
    output += strings[index];
    return output;
}

function schemaToModel(schema) {
  const entity = {};
  Object.keys(schema.properties).forEach((propertyName) => {
    const property = schema.properties[propertyName];
    if (property.type === 'object') {
      entity[propertyName] = schemaToModel(property);
    } else if ('default' in property) {
      entity[propertyName] = property.default;
    } else if (property.type === 'array') {
      entity[propertyName] = [];
    } else if (property.type === 'boolean') {
      entity[propertyName] = false;
    } else if (property.type === 'number') {
      entity[propertyName] = 0;
    } else if (property.type === 'string') {
      entity[propertyName] = null;
    }
    else{
      entity[propertyName] = null;
    }
  });
  return entity;
}

export default html;
export { schemaToModel }
