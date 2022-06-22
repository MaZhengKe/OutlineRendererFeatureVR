# URP管线下VR适用的外轮廓描边
- - - -
因为很多模型不给平滑法线，没法用法线外扩，只能用类似后处理的方式来描边了.

好处是通用性强，任何模型都能用，弊端就是性能消耗大一点。 VR下大概增加了0.4ms

主要参考自 https://zhuanlan.zhihu.com/p/170241589 在此基础上改为URP的RenderFeature，并支持了VR中SinglePass

大体思路是先绘制遮罩，再遮罩外扩。

 - 支持多重RenderFeature的叠加
 - mass抗锯齿
 - 遮挡显示与否
 - 支持VR中SinglePass渲染模式
 - 采用了RenderLayerMask，避免了占用LayerMask

![全部显示](/All.png)
<div style="text-align: center;"> 全部显示 </div>
<br>

![遮挡不显示](/Hide.png)
<div style="text-align: center;"> 遮挡不显示 </div>

![混合多重](/mult.png)
<div style="text-align: center;"> 多重RenderFeature叠加 </div>