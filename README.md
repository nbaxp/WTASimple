# 说明

## C# 到 JSON Schema 类型映射

### 值类型

1. bool?=>boolean[nullable]
1. int/long?=>integer[nullable]
1. float/double/decimal?=>number[nullable]
1. Guid/DateTime/Enum?=>string[nullable]

### 引用类型

1. string=>string
1. object=>object
1. IEnumerable=>array

## 格式化

format 用于格式验证

## 输入

input 用于输入控件
