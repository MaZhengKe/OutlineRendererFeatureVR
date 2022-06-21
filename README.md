# URP管线下VR适用的外轮廓描边
- - - -
因为很多模型不给平滑法线，没法用法线外扩，只能用类似后处理的方式来描边了.

好处是通用性强，任何模型都能用，弊端就是性能消耗大一点。

大体思路先绘制遮罩，再遮罩外扩。

![全部显示](/All.png)
<div style="text-align: center;"> 全部显示 </div>
<br>

![遮挡不显示](/Hide.png)
<div style="text-align: center;"> 遮挡不显示 </div>