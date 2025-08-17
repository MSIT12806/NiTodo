# NiTodo

建立一個簡單的代辦事項應用程式。

## Sprint 1
- [x] 新增代辦事項
- [x] 刪除代辦事項
- [x] 代辦事項為完成
- [x] 代辦事項標記為完成後，五秒內會從代辦清單中消失

## Sprint 2
- [ ] 代辦事項的完成條件設定[註：[為什麼不使用子代辦事項的概念](#為什麼不使用子代辦事項的概念)]
    - 要完成另一個代辦事項，才能完成這個代辦事項
    - 可能還有別的完成條件的設定，所以要保留擴充空間
- [ ] 為代辦事項加上標籤
- [ ] 各種條件篩選查詢代辦事項

## 其他內容

### 為什麼不使用子代辦事項的概念
1. 希望畫面保持簡潔


已完成的優化：

避免每次重建 List<ListBoxItem>：改用 ObservableCollection + CollectionView。
停止頻繁重新指派 ItemsSource：改 _todoView.Refresh()。
虛擬化啟用 + Recycling + CacheLength + Pixel Scroll + DeferredScrolling。
取消 SizeToContent=Height，降低大量項目時的重新量測。
排序抽出 comparer（少一次 LINQ 重新建立新清單）。
Highlight 計時器不再重設 ItemsSource，只 Refresh。
移除多餘 AddToPanel/手動 Grid 建構。
單筆完成/取消後不整個重建（仍 Refresh，但不重建集合）。
尚未實作（仍有空間）：

Diff 可見集合（現在 Refresh 仍全量 Filter+Sort）。
分批增量顯示（目前一次顯示全部）。
篩選/排序背景計算 + UI 差異套用。
節流/合併多次 Refresh 請求。
Tag 集合與排序鍵快取（仍每次動態算）。
Highlight 精準更新（仍全表 Refresh）。
INotifyPropertyChanged / ViewModel（目前無，單筆變動仍全表 predicate）。
預解析 / 快取 Tags、Completed 五秒判斷旗標。
移除 CollectionView.Filter，改手寫 Diff（必要時）。
Template 瘦身（Grid→更輕容器、移除 Style Triggers）。