export default {
  template: `{{$route.fullPath}}<el-pagination
  v-model:current-page="currentPage4"
  v-model:page-size="pageSize4"
  :page-sizes="[100, 200, 300, 400]"
  :small="small"
  layout="total, sizes, prev, pager, next, jumper"
  :total="400"
/>`,
};
