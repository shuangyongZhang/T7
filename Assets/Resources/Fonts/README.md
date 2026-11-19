# iTextSharp Unity Android 字体设置说明

## 字体文件准备

### 1. 获取中文字体文件
- 下载 `simsun.ttf` 或其他中文字体文件（如微软雅黑、宋体等）
- 确保字体文件支持中文字符

### 2. 放置字体文件
在 Unity 项目中创建以下目录结构：
```
Assets/Resources/Fonts/
```

将字体文件重命名为 `simsun.ttf` 并放置在此目录下。

### 3. 字体加载逻辑

代码会按以下优先级尝试加载字体：

#### Android 平台：
1. **系统字体**（推荐）：
   - `/system/fonts/DroidSansFallback.ttf`
   - `/system/fonts/NotoSansCJK-Regular.ttc`
   - `/system/fonts/SourceHanSans-Regular.otf`

2. **Resources 字体**：
   - `Assets/Resources/Fonts/simsun.ttf`

#### 其他平台：
1. **Resources 字体**
2. **系统字体目录**

### 4. 编译设置
- 确保字体文件被包含在 APK 中
- 在 Player Settings 中检查字体相关设置

### 5. 测试
在 Android 设备上测试 PDF 生成，确保中文正常显示。

## 注意事项
- Android 系统字体路径可能因设备和系统版本而异
- 如果系统字体不可用，确保 Resources 中有字体文件
- 字体文件大小会影响 APK 大小，建议使用压缩的 TTF 格式