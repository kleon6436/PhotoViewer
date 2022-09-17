/*!
 * @file	ImageReader.h
 * @author	kleon6436
 */

#pragma once

#include "ImageData.h"
#include "IImageController.h"
#include <memory>

namespace Kchary::ImageController::Library
{
	class ImageReader final
	{
	public:
		ImageReader();

		/*!
		* @brief 画像を読み込み、画像サイズを取得する
		* @param imagePath: 画像ファイルパス
		* @param imageReadSettings	画像読み込み設定
		* @param imageSize	画像サイズ(out)
		* @return 成功: True, 失敗: False
		*/
		bool LoadImageAndGetImageSize(const wchar_t* imagePath, const ImageReadSettings& imageReadSettings, int& imageSize);

		/*!
		 * @brief	画像データを取得する
		 * @param	imageData: 画像データ
		 * @return	成功: True, 失敗: False
		 */
		bool GetImageData(ImageData& imageData);

		/*!
		 * @brief	画像のサムネイルデータを取得する
		 * @param	imageData: 画像データ
		 * @return	成功: True, 失敗: False
		 */
		bool GetThumbnailImageData(ImageData& imageData);

	private:
		/*!
		 * @brief	wcharをchar配列に変換する
		 * @param	imagePath: wchar配列の画像ファイルパス
		 * @return	char配列のユニークポインタ(画像ファイルパス)
		 */
		std::unique_ptr<char[]> ConvertWcharToChar(const wchar_t* imagePath);

		ImageReadSettings m_imageReadSettings;	//!< 画像読み込み設定
		std::unique_ptr<IImageController> m_rawImageController;		//!< RAW画像読み込み用インスタンス
		std::unique_ptr<IImageController> m_normalImageController;	//!< 通常の画像読み込み用インスタンス
	};
}