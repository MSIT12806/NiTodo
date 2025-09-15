using NiScheduleApp.ValueObjects.ScheduleFrequencies;

namespace NiSchedule.App
{
    public class ScheduleItem
    {
        /*
         * #### 排程產生任務
- 主畫面頂部控制項有.一個名稱為"排程"的按鈕
- 點擊"排程"按鈕，彈出一個視窗，該視窗由兩個區塊組成
    - 新增區塊
        - 任務名稱輸入框
        - 預計執行時間：
            - 核取方塊
                - 每年
                    - 核取方塊
                        - 一到十二月
                    - 其他內容同"每月"
                - 每月
                    - 輸入框
                        - 日期(1~31)
                    - 其他內容同"每日"
                - 每週
                    - 核取方塊
                        - 週日到週六
                    - 其他內容同"每日"
                - 每日
                    - 小時(0~23)輸入框
                    - 分鐘(0~59)輸入框
        - 儲存按鈕：點擊後儲存該任務
    - 任務清單區塊
        - 顯示目前所有的排程任務
        - 每個任務列有一個按鈕
            - 刪除按鈕：點擊後可刪除該任務
- 固定檢查未來的某個時刻(每日/每週/每月/每年)是否有該名稱的待辦事項，如果沒有，則新增該待辦事項
    - 任務名稱(也就是待辦事項內容)
    - 到期時間
         */
        public string Id { get; set; }
        public string Content { get; set; }
        public ScheduleFrequency ScheduleFrequency { get; set; }

    }
}
