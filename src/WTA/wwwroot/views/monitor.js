import html from "html";
import { ref, reactive, onMounted, onUnmounted } from "vue";
import { useRoute } from "vue-router";
import Chart from "../components/chart/index.js";
import { get, post } from "../request/index.js";
import { ElMessage, dayjs } from "element-plus";
import { bytesFormat, persentFormat } from "../utils/index.js";
import PubSub from "pubsub-js";

export default {
  components: { Chart },
  template: html` <template v-if="model">
    <el-row :gutter="20" style="margin-bottom:20px;">
      <el-col :span="24">
        <el-card class="box-card">
          <template #header>
            <div class="card-header">
              <span>操作系统</span>
            </div>
          </template>
          <el-descriptions border direction="vertical" :column="4">
            <el-descriptions-item label="架构">{{model.osArchitecture}}</el-descriptions-item>
            <el-descriptions-item label="名称">{{model.osDescription}}</el-descriptions-item>
            <el-descriptions-item label="主机">{{model.hostName}}</el-descriptions-item>
            <el-descriptions-item label="用户">{{model.userName}}</el-descriptions-item>
            <el-descriptions-item label="时间">{{dayjs(model.serverTime).format()}}</el-descriptions-item>
            <el-descriptions-item label="地址">{{model.hostAddresses}}</el-descriptions-item>
            <el-descriptions-item label="进程">{{model.processCount}}</el-descriptions-item>
            <el-descriptions-item label="线程">{{model.threadCount}}</el-descriptions-item>
            <el-descriptions-item label="名称">{{model.driveName}}</el-descriptions-item>
            <el-descriptions-item label="大小">{{bytesFormat(model.drivieTotalSize)}}</el-descriptions-item>
            <el-descriptions-item label="剩余">{{bytesFormat(model.driveAvailableFreeSpace)}}</el-descriptions-item>
            <el-descriptions-item label="占用">{{persentFormat(1-model.driveAvailableFreeSpace/model.drivieTotalSize)}}</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>
    <el-row :gutter="20" style="margin-bottom:20px;">
      <el-col :span="12">
        <el-card class="box-card">
          <template #header>
            <div class="card-header">
              <span>处理器 {{model.processorCount}} </span>
            </div>
          </template>
          <chart :option="cpuModel" height="300px" />
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card class="box-card">
          <template #header>
            <div class="card-header">
              <span>内存 {{bytesFormat(model.totalMemory)}}</span>
            </div>
          </template>
          <chart :option="memoryModel" height="300px" />
        </el-card>
      </el-col>
    </el-row>
    <el-row :gutter="20" style="margin-bottom:20px;">
      <el-col :span="12">
        <el-card class="box-card">
          <template #header>
            <div class="card-header">
              <span>磁盘</span>
            </div>
          </template>
          <el-descriptions border direction="vertical">
            <el-descriptions-item label="读取">{{bytesFormat(model.diskRead)}}</el-descriptions-item>
            <el-descriptions-item label="写入">{{bytesFormat(model.diskWrite)}}</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card class="box-card">
          <template #header>
            <div class="card-header">
              <span>网络</span>
            </div>
          </template>
          <el-descriptions border direction="vertical">
            <el-descriptions-item label="下载">{{bytesFormat(model.netReceived)}}</el-descriptions-item>
            <el-descriptions-item label="上传">{{bytesFormat(model.netSent)}}</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>
    <el-row :gutter="20" style="margin-bottom:20px;">
      <el-col :span="24">
        <el-card class="box-card">
          <template #header>
            <div class="card-header">
              <span>.NET</span>
            </div>
          </template>
          <el-descriptions border direction="vertical" :column="4">
            <el-descriptions-item label="名称">{{model.frameworkDescription}}</el-descriptions-item>
            <el-descriptions-item label="已分配内存">{{bytesFormat(model.gcTotalMemory)}}</el-descriptions-item>
            <el-descriptions-item label="可回收对象">{{model.finalizationPendingCount}}</el-descriptions-item>
            <el-descriptions-item label="堆大小">{{bytesFormat(model.heapSizeBytes)}}</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>
    <el-row :gutter="20" style="margin-bottom:20px;">
      <el-col :span="24">
        <el-card class="box-card">
          <template #header>
            <div class="card-header">
              <span>应用</span>
            </div>
          </template>
          <el-descriptions border direction="vertical" :column="5">
            <el-descriptions-item label="架构">{{model.processArchitecture}}</el-descriptions-item>
            <el-descriptions-item label="启动时间">{{dayjs(model.processStartTime).format()}}</el-descriptions-item>
            <el-descriptions-item label="启动位置">{{model.processFileName}}</el-descriptions-item>
            <el-descriptions-item label="参数">{{model.processArguments}}</el-descriptions-item>
            <el-descriptions-item label="进程Id">{{model.processId}}</el-descriptions-item>
            <el-descriptions-item label="进程名称">{{model.processName}}</el-descriptions-item>
            <el-descriptions-item label="CPU">{{persentFormat(model.processCpuUsage)}}</el-descriptions-item>
            <el-descriptions-item label="内存">{{bytesFormat(model.processMemory)}}</el-descriptions-item>
            <el-descriptions-item label="磁盘读写"
              >{{bytesFormat(model.processDiskRead)}} / {{bytesFormat(model.processDiskWrite)}}</el-descriptions-item
            >
            <el-descriptions-item label="线程">{{model.processThreadCount}}</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>
  </template>`,
  setup(props) {
    const route = useRoute();
    const baseUrl = `${route.meta.path}`.substring(1);
    const url = `${baseUrl}/index`;
    const schema = reactive({});
    const model = reactive({});
    //
    const cpuModel = reactive({
      title: {
        text: "处理器",
      },
      xAxis: {
        type: "category",
        data: Object.keys(Array(30).fill()),
      },
      yAxis: {
        type: "value",
        min: 0,
        max: 100,
      },
      series: [
        {
          data: [],
          type: "line",
          smooth: true,
        },
      ],
    });

    const memoryModel = reactive({
      title: {
        text: "内存",
      },
      xAxis: {
        type: "category",
        data: Object.keys(Array(30).fill()),
      },
      yAxis: {
        type: "value",
        min: 0,
        max: 100,
      },
      series: [
        {
          data: [],
          type: "bar",
          smooth: true,
        },
      ],
    });
    //
    const onMonitor = (method, data) => {
      Object.assign(model, data);
      // cpu
      if (cpuModel.series[0].data.length > 30) {
        cpuModel.series[0].data.shift();
      }
      cpuModel.title.text = `处理器 ${persentFormat(model.cpuUsage)}`;
      cpuModel.series[0].data.push(model.cpuUsage * 100);
      // memory
      if (memoryModel.series[0].data.length > 30) {
        memoryModel.series[0].data.shift();
      }
      memoryModel.title.text = `内存 ${persentFormat(model.memoryUsage)}`;
      memoryModel.series[0].data.push(model.memoryUsage * 100);
    };
    onMounted(async () => {
      Object.assign(schema, (await get(url)).data);
      Object.assign(model, (await post(url)).data);
      PubSub.subscribe("monitor", onMonitor);
    });
    onUnmounted(() => {
      PubSub.unsubscribe(onMonitor);
    });
    //
    return {
      schema,
      model,
      cpuModel,
      memoryModel,
      dayjs,
      bytesFormat,
      persentFormat,
    };
  },
};
