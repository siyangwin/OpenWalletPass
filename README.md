# OpenWalletPass
OpenWalletPass 旨在简化 Apple Wallet 和 Google Wallet 中票证、卡片与通行证的创建与集成流程。无论是活动门票、会员卡还是交通票据，本项目都提供了一个清晰、易于扩展的 Demo 示例，帮助开发者快速上手。

### Apple示例
PassKit安装界面

![IMG_0017](https://github.com/user-attachments/assets/e2e163b9-2d53-40fc-977a-6f5bd1834e97)

Passkit背后页面

![IMG_0018](https://github.com/user-attachments/assets/ad6e9faa-2bd3-4137-aa3c-1fbcdc7347e8)

Apple官方文档：https://developer.apple.com/documentation/walletpasses/creating-the-source-for-a-pass


生成的文件，会保存在目录下，可通过Email附件发送给用户，或者放置在应用程序给用户下载。
下载需要使用API中提供的下载方法，解决了第三方浏览器可能不触发Wallet应用程序的问题。

步骤(完整流程Demo)：
1. 按规则生成Passkit文件
2. 发送此Passkit文件给用户
3. Apple回调API, 告知用户设备唯一编号和PushToken
4. 后端更新Passkit后，需要通过PushToken通知Apple触发更新。
5. Apple回调API，发送用户设备唯一编号获取SN检验
6. Apple回调API，刷新获取最新PassKit文件
7. Apple回调API，通知用户已经手动删除Passkit文件，触发解绑。
8. Apple回调API，发送Log信息给后台。

### Google示例
