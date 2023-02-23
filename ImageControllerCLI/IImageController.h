/*!
 * @file	IImageController.h
 * @author	kleon6436
 */

#pragma once

#include "ImageData.h"

class IImageController
{
public:
	/*!
	 * @brief コンストラクタ
	 */
	IImageController() = default;

	/*!
	 * @brief デストラクタ
	 */
	virtual ~IImageController() = default;

	/*!
	 * @brief	画像データを取得する
	 * @param	path							画像パス
	 * @param	imageReadSettings	画像設定
	 * @param	imageData				画像データ(out)
	 * @return	成功: True, 失敗: False
	 */
	virtual bool GetImageData(const wchar_t* path, const ImageReadSettings& imageReadSettings, ImageData& imageData) = 0;
};