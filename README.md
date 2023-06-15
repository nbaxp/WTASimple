# 说明

## 模块划分

1. 一个模块有多个数据库上下文
1. 一个数据库上下文有多个实体配置和种子配置
1. 每个实体配置和种子配置关联到一个数据上下文

## 权限自动生成

1. IResource 标记资源，IResourceService\<TResource\> where TResource:IResource 标记服务
1. Entity 默认实现 IResource 接口
1. 泛型控制器 GenericControlle 默认实现 Entity 的增删改查导入导出操作，其他自定义操作可以继承泛型控制器
1. 非 Entity 资源可以手动实现 IResource 和 IResourceService 接口

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
