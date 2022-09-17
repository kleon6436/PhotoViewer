/*!
 * @file	ImageControlLibrary.h
 * @author	kleon6436
 */

# pragma once

#ifdef __cplusplus
#define DllExport extern "C" __declspec(dllexport)
#else
#define DllExport __declspec(dllexport)
#endif

#include "ImageData.h"
#include <iostream>

namespace Kchary::ImageController::Library
{
	/*!
	* @brief 画像を読み込み、画像サイズを取得する
	* @param imagePath: 画像ファイルパス
	* @param imageReadSettings	画像読み込み設定
	* @param imageSize	画像サイズ(out)
	* @return 成功: True, 失敗: False
	*/
	DllExport bool LoadImageAndGetImageSize(const wchar_t* imagePath, const ImageReadSettings& imageReadSettings, int& imageSize);

	/*!
	 * @brief	画像データを取得する
	 * @param	imageData: 画像データ
	 * @return	成功: True, 失敗: False
	 */
	DllExport bool GetImageData(ImageData& imageData);

	/*!
	 * @brief	画像のサムネイルデータを取得する
	 * @param	imageData: 画像データ
	 * @return	成功: True, 失敗: False
	 */
	DllExport bool GetThumbnailImageData(ImageData& imageData);
}