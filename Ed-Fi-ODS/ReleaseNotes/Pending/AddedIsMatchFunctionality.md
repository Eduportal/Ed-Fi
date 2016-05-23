* Added `IsMatch` flag back to Identity

>  IsMatch goes true if there is a big difference between the weight of the first found identity and the rest in the list. 

> The difference is hard coded as %50. The existing *match threshold* is still in used. 

> We ignore found items with the weight less than 3.0.

> In addition, if the weight for the first found item is more than 6.0 `isMatch` is set to true

