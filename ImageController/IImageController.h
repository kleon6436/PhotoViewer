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
	* @brief 画像を読み込み、画像サイズを取得する
	* @param path: 画像ファイルパス
	* @param imageReadSettings	画像読み込み設定
	* @param imageSize	画像サイズ(out)
	* @return 成功: True, 失敗: False
	*/
	virtual bool LoadImageAndGetImageSize(const wchar_t* path, const ImageReadSettings& imageReadSettings, int& imageSize) = 0;

	/*!
	 * @brief	画像データを取得する
	 * @param	imageData: 画像データ(out)
	 * @return	成功: True, 失敗: False
	 */
	virtual bool GetImageData(ImageData& imageData) = 0;

	/*!
	 * @brief	サムネイル画像を取得する
	 * @param	imageData: 画像データ(out)
	 * @return	成功: True, 失敗: False
	 */
	virtual bool GetThumbnailImageData(ImageData& imageData) = 0;
};