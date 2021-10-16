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
	 * @param	path: 画像ファイルパス
	 * @param	imageData: 画像データ(out)
	 * @return	成功: 0, 失敗: -1
	 */
	virtual int GetImageData(const char* path, ImageData& imageData) const = 0;

	/*!
	 * @brief	サムネイル画像を取得する
	 * @param	path: 画像ファイルパス
	 * @param	resizeLongSideLength: リサイズする長辺の長さ
	 * @param	imageData: 画像データ(out)
	 * @return	成功: 0, 失敗: -1
	 */
	virtual int GetThumbnailImageData(const char* path, int resizeLongSideLength, ImageData& imageData) const = 0;
};